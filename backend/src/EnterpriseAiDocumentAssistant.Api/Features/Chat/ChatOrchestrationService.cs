using EnterpriseAiDocumentAssistant.Api.Audit;
using EnterpriseAiDocumentAssistant.Api.AiGateway;
using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.Guardrails;
using EnterpriseAiDocumentAssistant.Api.Planner;
using EnterpriseAiDocumentAssistant.Api.PromptOrchestration;
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
    private readonly IAssistantMessageAdapter assistantMessageAdapter;

    public ChatOrchestrationService(
        IDocumentAssistantPromptOrchestrator promptOrchestrator,
        IAiGateway aiGateway,
        IStructuredAssistantResponseValidator structuredResponseValidator,
        IChatGuardrailEvaluator chatGuardrailEvaluator,
        IAuditLogger auditLogger,
        IAgentPlanner agentPlanner,
        IAssistantMessageAdapter assistantMessageAdapter)
    {
        this.promptOrchestrator = promptOrchestrator;
        this.aiGateway = aiGateway;
        this.structuredResponseValidator = structuredResponseValidator;
        this.chatGuardrailEvaluator = chatGuardrailEvaluator;
        this.auditLogger = auditLogger;
        this.agentPlanner = agentPlanner;
        this.assistantMessageAdapter = assistantMessageAdapter;
    }

    public async Task<ChatOrchestrationResult> BuildValidatedMessageAsync(
        ChatRequest request,
        CancellationToken cancellationToken)
    {
        // Step 1: input guardrails run before planner, skills, tools, or model execution.
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

        // Step 2: planner decides whether the request should use a specialized capability.
        var plan = await agentPlanner.PlanAsync(
            new AgentPlanRequest(request.Message, request.DocumentId, request.AiProvider),
            cancellationToken);

        // Step 3: planned skill/workflow results are normalized back to the common assistant contract.
        var plannedMessage = await assistantMessageAdapter.TryBuildFromPlanAsync(
            request,
            plan,
            cancellationToken);
        if (plannedMessage is not null)
        {
            return Validate(assistantMessageAdapter.AttachDocumentCitations(request, plannedMessage));
        }

        // Step 4: normal chat path builds a prompt and calls the selected model provider through AI Gateway.
        var prompt = promptOrchestrator.BuildAssistantPrompt(request);
        var modelResponse = await aiGateway.GenerateChatResponseAsync(
            new ChatModelRequest(prompt, request.AiProvider),
            cancellationToken);

        // Step 5: attach source context and validate the structured assistant response before returning to HTTP.
        return Validate(assistantMessageAdapter.AttachDocumentCitations(request, modelResponse.Message));
    }

    private ChatOrchestrationResult Validate(StructuredAssistantMessage structuredMessage)
    {
        // Output validation keeps controller code free from JSON contract details.
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
