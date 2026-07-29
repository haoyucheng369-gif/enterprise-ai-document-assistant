using EnterpriseAiDocumentAssistant.Api.Audit;
using EnterpriseAiDocumentAssistant.Api.AiGateway;
using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.Guardrails;
using EnterpriseAiDocumentAssistant.Api.Planner;
using EnterpriseAiDocumentAssistant.Api.PromptOrchestration;
using EnterpriseAiDocumentAssistant.Api.Rag;
using EnterpriseAiDocumentAssistant.Api.StructuredOutput;

namespace EnterpriseAiDocumentAssistant.Api.Chat;

public sealed class ChatOrchestrationService : IChatOrchestrationService
{
    private readonly IDocumentAssistantPromptOrchestrator promptOrchestrator;
    private readonly IAiGateway aiGateway;
    private readonly IStructuredAssistantResponseValidator structuredResponseValidator;
    private readonly IChatGuardrailEvaluator chatGuardrailEvaluator;
    private readonly IAuditLogger auditLogger;
    private readonly IAgentPlanner agentPlanner;
    private readonly IPlannedCapabilityExecutor plannedCapabilityExecutor;
    private readonly IRagService ragService;

    public ChatOrchestrationService(
        IDocumentAssistantPromptOrchestrator promptOrchestrator,
        IAiGateway aiGateway,
        IStructuredAssistantResponseValidator structuredResponseValidator,
        IChatGuardrailEvaluator chatGuardrailEvaluator,
        IAuditLogger auditLogger,
        IAgentPlanner agentPlanner,
        IPlannedCapabilityExecutor plannedCapabilityExecutor,
        IRagService ragService)
    {
        this.promptOrchestrator = promptOrchestrator;
        this.aiGateway = aiGateway;
        this.structuredResponseValidator = structuredResponseValidator;
        this.chatGuardrailEvaluator = chatGuardrailEvaluator;
        this.auditLogger = auditLogger;
        this.agentPlanner = agentPlanner;
        this.plannedCapabilityExecutor = plannedCapabilityExecutor;
        this.ragService = ragService;
    }

    public async Task<ChatOrchestrationResult> BuildValidatedMessageAsync(
        ChatRequest request,
        CancellationToken cancellationToken)
    {
        // Main chat flow: guardrails -> explicit skill/workflow route -> default RAG answer -> output validation.
        var guardrailResult = await TryBuildGuardrailResponseAsync(request, cancellationToken);
        if (guardrailResult is not null)
        {
            return guardrailResult;
        }

        var plannedMessage = await TryBuildPlannedCapabilityMessageAsync(request, cancellationToken);
        if (plannedMessage is not null)
        {
            return Validate(plannedCapabilityExecutor.AttachDocumentCitations(request, plannedMessage));
        }

        var documentAnswer = await GenerateDocumentAnswerAsync(request, cancellationToken);
        return Validate(documentAnswer);
    }

    private async Task<ChatOrchestrationResult?> TryBuildGuardrailResponseAsync(
        ChatRequest request,
        CancellationToken cancellationToken)
    {
        // Block unsafe input before routing, retrieval, or provider calls.
        var guardrailEvaluation = await chatGuardrailEvaluator.EvaluateAsync(request, cancellationToken);
        if (guardrailEvaluation.Classification.NeedsReview)
        {
            RecordSafetyAudit(request, guardrailEvaluation.Classification);
        }

        if (guardrailEvaluation.IsBlocked)
        {
            RecordSafetyAudit(request, guardrailEvaluation.Classification);
            return Validate(guardrailEvaluation.Response
                ?? throw new InvalidOperationException("Guardrail response was not created."));
        }

        return null;
    }

    private async Task<StructuredAssistantMessage?> TryBuildPlannedCapabilityMessageAsync(
        ChatRequest request,
        CancellationToken cancellationToken)
    {
        // Only explicit application actions return a skill/workflow response; normal questions return null.
        var plan = await agentPlanner.PlanAsync(
            new AgentPlanRequest(request.Message, request.DocumentId, request.AiProvider),
            cancellationToken);

        return await plannedCapabilityExecutor.ExecutePlanAsync(
            request,
            plan,
            cancellationToken);
    }

    private async Task<StructuredAssistantMessage> GenerateDocumentAnswerAsync(
        ChatRequest request,
        CancellationToken cancellationToken)
    {
        // RAG returns plain-text evidence ready to become prompt context.
        var retrieval = await ragService.RetrieveAsync(request, cancellationToken);

        if (retrieval.Status == RagRetrievalStatus.InsufficientEvidence)
        {
            return BuildInsufficientEvidenceMessage(request);
        }

        // Retrieved source text is inserted into the prompt; embedding vectors never reach the chat model.
        var prompt = retrieval.HasEvidence
            ? promptOrchestrator.BuildAssistantPrompt(request, retrieval.PromptContext)
            : promptOrchestrator.BuildAssistantPrompt(request);

        var modelResponse = await aiGateway.GenerateChatResponseAsync(
            new ChatModelRequest(prompt, request.AiProvider),
            cancellationToken);

        return retrieval.HasEvidence
            ? modelResponse.Message with { Citations = retrieval.Citations }
            : plannedCapabilityExecutor.AttachDocumentCitations(request, modelResponse.Message);
    }

    private static StructuredAssistantMessage BuildInsufficientEvidenceMessage(ChatRequest request)
    {
        var prefersChinese = EnterpriseAssistantPromptDefaults.PrefersChinese(request.Message);

        return prefersChinese
            ? new StructuredAssistantMessage(
                "当前文档中没有检索到与这个问题足够相关的内容，因此我无法基于文档给出可靠答案。",
                "low",
                [],
                [
                    "换一种更具体的问法",
                    "检查是否选择了正确的文档",
                    "查看文档预览"
                ])
            : new StructuredAssistantMessage(
                "The current document does not contain sufficiently relevant evidence for this question, so I cannot provide a reliable document-grounded answer.",
                "low",
                [],
                [
                    "Ask a more specific question",
                    "Check that the correct document is selected",
                    "Review the document preview"
                ]);
    }

    private ChatOrchestrationResult Validate(StructuredAssistantMessage structuredMessage)
    {
        // Enforce the stable response contract before returning to the controller.
        var validationResult = structuredResponseValidator.Validate(structuredMessage);

        return validationResult.IsValid
            ? new ChatOrchestrationResult(structuredMessage, [])
            : new ChatOrchestrationResult(null, validationResult.Errors);
    }

    private void RecordSafetyAudit(
        ChatRequest request,
        SafetyClassification classification)
    {
        auditLogger.Record(new AuditEventRequest(
            "safety",
            classification.Decision,
            "api/chat",
            !classification.IsBlocked,
            0,
            new Dictionary<string, string>
            {
                ["documentId"] = request.DocumentId ?? string.Empty,
                ["riskType"] = classification.RiskType,
                ["confidence"] = classification.Confidence.ToString("0.00"),
                ["signals"] = string.Join(",", classification.Signals)
            }));
    }
}
