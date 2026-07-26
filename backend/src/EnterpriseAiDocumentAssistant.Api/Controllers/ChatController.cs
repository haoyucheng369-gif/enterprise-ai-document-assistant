using System.Diagnostics;
using EnterpriseAiDocumentAssistant.Api.Audit;
using EnterpriseAiDocumentAssistant.Api.AiGateway;
using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.Guardrails;
using EnterpriseAiDocumentAssistant.Api.Planner;
using EnterpriseAiDocumentAssistant.Api.PromptOrchestration;
using EnterpriseAiDocumentAssistant.Api.Services;
using EnterpriseAiDocumentAssistant.Api.Skills;
using EnterpriseAiDocumentAssistant.Api.StructuredOutput;
using EnterpriseAiDocumentAssistant.Api.Workflows;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiDocumentAssistant.Api.Controllers;

[ApiController]
[Route("api/chat")]
public sealed class ChatController : ControllerBase
{
    private readonly IDocumentAssistantPromptOrchestrator promptOrchestrator;
    private readonly IAiGateway aiGateway;
    private readonly IStructuredAssistantResponseValidator structuredResponseValidator;
    private readonly IChatGuardrailEvaluator chatGuardrailEvaluator;
    private readonly IAuditLogger auditLogger;
    private readonly IApplicationDocumentProvider applicationDocumentProvider;
    private readonly IAgentPlanner agentPlanner;
    private readonly ISummarySkill summarySkill;
    private readonly IRiskAnalysisSkill riskAnalysisSkill;
    private readonly IEmailDraftSkill emailDraftSkill;
    private readonly IClassificationSkill classificationSkill;
    private readonly IResumeReviewSkill resumeReviewSkill;
    private readonly IDocumentReviewWorkflow documentReviewWorkflow;

    public ChatController(
        IDocumentAssistantPromptOrchestrator promptOrchestrator,
        IAiGateway aiGateway,
        IStructuredAssistantResponseValidator structuredResponseValidator,
        IChatGuardrailEvaluator chatGuardrailEvaluator,
        IAuditLogger auditLogger,
        IApplicationDocumentProvider applicationDocumentProvider,
        IAgentPlanner agentPlanner,
        ISummarySkill summarySkill,
        IRiskAnalysisSkill riskAnalysisSkill,
        IEmailDraftSkill emailDraftSkill,
        IClassificationSkill classificationSkill,
        IResumeReviewSkill resumeReviewSkill,
        IDocumentReviewWorkflow documentReviewWorkflow)
    {
        this.promptOrchestrator = promptOrchestrator;
        this.aiGateway = aiGateway;
        this.structuredResponseValidator = structuredResponseValidator;
        this.chatGuardrailEvaluator = chatGuardrailEvaluator;
        this.auditLogger = auditLogger;
        this.applicationDocumentProvider = applicationDocumentProvider;
        this.agentPlanner = agentPlanner;
        this.summarySkill = summarySkill;
        this.riskAnalysisSkill = riskAnalysisSkill;
        this.emailDraftSkill = emailDraftSkill;
        this.classificationSkill = classificationSkill;
        this.resumeReviewSkill = resumeReviewSkill;
        this.documentReviewWorkflow = documentReviewWorkflow;
    }

    [HttpPost]
    [ProducesResponseType<ChatResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChatResponse>> Post(
        ChatRequest request,
        CancellationToken cancellationToken)
    {
        // Chat Step 1: HTTP entry point used by the React assistant panel.
        // This method owns request validation, delegates the AI flow, then wraps the final message for the UI.
        var stopwatch = Stopwatch.StartNew();

        // Chat Step 2: reject invalid HTTP input before any guardrail, planner, or model call happens.
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            ModelState.AddModelError(nameof(request.Message), "Message is required.");
            return ValidationProblem(ModelState);
        }

        // Chat Step 3: run the complete assistant flow and receive one validated structured message.
        var structuredMessage = await BuildValidatedStructuredMessageAsync(request, cancellationToken);
        if (structuredMessage.Result is not null)
        {
            // Chat Step 3b: blocked guardrails or validation failures return early as HTTP results.
            return structuredMessage.Result;
        }

        // Chat Step 4: convert the structured assistant payload into the chat message shape used by the frontend.
        var message = structuredMessage.Value
            ?? throw new InvalidOperationException("Structured message was not created.");
        var response = new MessageResponse(
            $"assistant-{Guid.NewGuid():N}",
            "assistant",
            message.Answer);

        // Chat Step 5: record the successful chat call and return the response to React.
        RecordChatAudit("chat_completed", "api/chat", request, true, stopwatch.ElapsedMilliseconds);
        return Ok(new ChatResponse(response, message));
    }

    [HttpPost("structured")]
    [ProducesResponseType<StructuredChatResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StructuredChatResponse>> Structured(
        ChatRequest request,
        CancellationToken cancellationToken)
    {
        // Structured endpoint exposes only the validated assistant payload for Swagger/debugging.
        var stopwatch = Stopwatch.StartNew();

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            ModelState.AddModelError(nameof(request.Message), "Message is required.");
            return ValidationProblem(ModelState);
        }

        var structuredMessage = await BuildValidatedStructuredMessageAsync(request, cancellationToken);
        if (structuredMessage.Result is not null)
        {
            return structuredMessage.Result;
        }

        var message = structuredMessage.Value
            ?? throw new InvalidOperationException("Structured message was not created.");

        RecordChatAudit("structured_chat_completed", "api/chat/structured", request, true, stopwatch.ElapsedMilliseconds);
        return Ok(new StructuredChatResponse(message));
    }

    [HttpPost("stream")]
    [Produces("text/plain")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Stream(ChatRequest request, CancellationToken cancellationToken)
    {
        // Streaming endpoint currently streams validated text chunks after the structured response is ready.
        var stopwatch = Stopwatch.StartNew();

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            ModelState.AddModelError(nameof(request.Message), "Message is required.");
            return ValidationProblem(ModelState);
        }

        Response.ContentType = "text/plain; charset=utf-8";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Append("X-Accel-Buffering", "no");

        var structuredMessage = await BuildValidatedStructuredMessageAsync(request, cancellationToken);
        if (structuredMessage.Result is not null)
        {
            return structuredMessage.Result;
        }

        var message = structuredMessage.Value
            ?? throw new InvalidOperationException("Structured message was not created.");

        foreach (var chunk in aiGateway.BuildResponseChunks(message))
        {
            await Response.WriteAsync(chunk, cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
            await Task.Delay(120, cancellationToken);
        }

        RecordChatAudit("stream_chat_completed", "api/chat/stream", request, true, stopwatch.ElapsedMilliseconds);
        return new EmptyResult();
    }

    private async Task<ActionResult<StructuredAssistantMessage>> BuildValidatedStructuredMessageAsync(
        ChatRequest request,
        CancellationToken cancellationToken)
    {
        // Flow Step 1: Input Guardrails classify the user request before planner, skills, or model execution.
        var guardrailEvaluation = await chatGuardrailEvaluator.EvaluateAsync(request, cancellationToken);
        if (guardrailEvaluation.Classification.NeedsReview)
        {
            // Flow Step 1a: needs_review is allowed in V1, but it is still recorded for audit/debugging.
            RecordSafetyAudit(request, guardrailEvaluation.Classification);
        }

        if (guardrailEvaluation.IsBlocked)
        {
            // Flow Step 1b: blocked input returns a controlled assistant message and skips planner/model calls.
            RecordSafetyAudit(request, guardrailEvaluation.Classification);
            return ValidateStructuredMessage(guardrailEvaluation.Response
                ?? throw new InvalidOperationException("Guardrail response was not created."));
        }

        // Flow Step 2: Planner decides whether this free-form chat message should route to a known capability.
        var plan = await agentPlanner.PlanAsync(
            new AgentPlanRequest(request.Message, request.DocumentId, request.AiProvider),
            cancellationToken);

        // Flow Step 3: If the route maps to a skill/workflow, execute it and adapt the result back to chat.
        var plannedMessage = await TryExecutePlannedRouteAsync(request, plan, cancellationToken);
        if (plannedMessage is not null)
        {
            // Flow Step 3b: skill/workflow output still goes through citations and output validation.
            return ValidateStructuredMessage(AttachDocumentCitations(request, plannedMessage));
        }

        // Flow Step 4: No specialized route was selected, so build the normal assistant prompt.
        var prompt = promptOrchestrator.BuildAssistantPrompt(request);

        // Flow Step 5: AI Gateway calls Mock/OpenAI/Azure OpenAI using the selected provider.
        var modelResponse = await aiGateway.GenerateChatResponseAsync(
            new ChatModelRequest(prompt, request.AiProvider),
            cancellationToken);

        // Flow Step 6: Attach current document citations, validate the structured output, then return upward.
        return ValidateStructuredMessage(AttachDocumentCitations(request, modelResponse.Message));
    }

    private async Task<StructuredAssistantMessage?> TryExecutePlannedRouteAsync(
        ChatRequest request,
        AgentPlanResponse plan,
        CancellationToken cancellationToken)
    {
        // Route Step 1: This switch is where planner output becomes real application behavior.
        // Route Step 2: Each route calls one skill/workflow, then the result is normalized for the Assistant UI.
        return plan.Route switch
        {
            "skills.summary" => ConvertSummaryToAssistantMessage(await summarySkill.RunAsync(
                new SummarySkillRequest(plan.DocumentId, request.AiProvider, request.Message),
                cancellationToken), request),
            "skills.risk-analysis" => ConvertRiskAnalysisToAssistantMessage(await riskAnalysisSkill.RunAsync(
                new RiskAnalysisSkillRequest(plan.DocumentId, request.AiProvider, request.Message),
                cancellationToken), request),
            "skills.email-draft" => ConvertEmailDraftToAssistantMessage(await emailDraftSkill.RunAsync(
                new EmailDraftSkillRequest(plan.DocumentId, "Prepare a concise follow-up email draft.", request.AiProvider),
                cancellationToken)),
            "skills.classification" => ConvertClassificationToAssistantMessage(await classificationSkill.RunAsync(
                new ClassificationSkillRequest(plan.DocumentId, request.AiProvider),
                cancellationToken)),
            "skills.resume-review" => ConvertResumeReviewToAssistantMessage(await resumeReviewSkill.RunAsync(
                new ResumeReviewSkillRequest(plan.DocumentId, request.Message, request.AiProvider),
                cancellationToken)),
            "workflows.document-review" => ConvertWorkflowToAssistantMessage(await documentReviewWorkflow.RunAsync(
                new DocumentReviewWorkflowRequest(plan.DocumentId, "Prepare a concise follow-up email draft.", request.AiProvider),
                cancellationToken)),
            _ => null
        };
    }

    private static StructuredAssistantMessage? ConvertSummaryToAssistantMessage(
        SummarySkillResponse? response,
        ChatRequest request)
    {
        // Skill-specific contracts are converted back to the generic assistant message shape for chat UI reuse.
        return response is null
            ? null
            : new StructuredAssistantMessage(
                response.Summary,
                "high",
                response.Sources,
                BuildSummarySuggestedActions(request));
    }

    private static StructuredAssistantMessage? ConvertRiskAnalysisToAssistantMessage(
        RiskAnalysisSkillResponse? response,
        ChatRequest request)
    {
        // Risk items become a compact assistant answer while preserving sources as citations.
        if (response is null)
        {
            return null;
        }

        var answer = response.Risks.Count == 0
            ? BuildNoRisksMessage(request)
            : string.Join(Environment.NewLine, response.Risks.Select(risk =>
                $"- {risk.Title} ({FormatSeverity(risk.Severity, request)}): {risk.Recommendation}"));

        return new StructuredAssistantMessage(
            answer,
            "high",
            response.Risks.Select(risk => risk.Source).ToArray(),
            BuildRiskSuggestedActions(request));
    }

    private static StructuredAssistantMessage? ConvertEmailDraftToAssistantMessage(EmailDraftSkillResponse? response)
    {
        // Email draft keeps subject/body together because the assistant panel is text-first.
        return response is null
            ? null
            : new StructuredAssistantMessage(
                $"{response.Subject}{Environment.NewLine}{Environment.NewLine}{response.Body}",
                "high",
                response.BasedOn,
                response.NextActions);
    }

    private static StructuredAssistantMessage? ConvertClassificationToAssistantMessage(ClassificationSkillResponse? response)
    {
        // Classification remains readable in chat, while the dedicated tab keeps richer classification fields.
        return response is null
            ? null
            : new StructuredAssistantMessage(
                $"Category: {response.Category}. Priority: {response.Priority}. {response.Reason}",
                response.Confidence >= 0.75 ? "high" : "medium",
                response.Sources,
                ["Summarize this document", "Analyze risks", "Generate resume review"]);
    }

    private static StructuredAssistantMessage? ConvertResumeReviewToAssistantMessage(ResumeReviewSkillResponse? response)
    {
        // Resume review content is Markdown; frontend can show it as a generated draft.
        return response is null
            ? null
            : new StructuredAssistantMessage(
                response.Content,
                "high",
                response.BasedOn,
                response.NextActions);
    }

    private static StructuredAssistantMessage? ConvertWorkflowToAssistantMessage(DocumentReviewWorkflowResponse? response)
    {
        // Workflow output combines several skill results into one chat-friendly summary.
        if (response is null)
        {
            return null;
        }

        var risks = response.RiskAnalysis.Risks.Count == 0
            ? "No major risk items were identified."
            : string.Join("; ", response.RiskAnalysis.Risks.Select(risk => $"{risk.Title} ({risk.Severity})"));

        return new StructuredAssistantMessage(
            $"""
            Workflow completed.

            Summary: {response.Summary.Summary}

            Risks: {risks}

            Email draft: {response.EmailDraft.Subject}
            {response.EmailDraft.Body}
            """,
            "high",
            response.Summary.Sources
                .Concat(response.RiskAnalysis.Risks.Select(risk => risk.Source))
                .Concat(response.EmailDraft.BasedOn)
                .Distinct()
                .ToArray(),
            ["Review citations", "Refine email draft", "Ask a follow-up question"]);
    }

    private StructuredAssistantMessage AttachDocumentCitations(
        ChatRequest request,
        StructuredAssistantMessage message)
    {
        // For now citations come from the selected document sections, not directly from the model.
        // Later RAG can replace this with retrieved chunks and stronger source ranking.
        if (string.IsNullOrWhiteSpace(request.DocumentId))
        {
            return message;
        }

        var document = applicationDocumentProvider.FindById(request.DocumentId);
        if (document is null)
        {
            return message;
        }

        var citations = document.Sections
            .Take(4)
            .Select(section => $"{section.Label} - {section.Title}: {Truncate(section.Body, 120)}")
            .ToArray();

        return message with
        {
            Citations = citations.Length > 0
                ? citations
                : [$"Document: {document.Title}"]
        };
    }

    private ActionResult<StructuredAssistantMessage> ValidateStructuredMessage(
        StructuredAssistantMessage structuredMessage)
    {
        // Output validation is the final safety check before the assistant response leaves the API.
        var validationResult = structuredResponseValidator.Validate(structuredMessage);

        if (validationResult.IsValid)
        {
            return structuredMessage;
        }

        return Problem(
            title: "StructuredOutputValidationFailed",
            detail: string.Join(" ", validationResult.Errors),
            statusCode: StatusCodes.Status502BadGateway);
    }

    private void RecordChatAudit(
        string action,
        string route,
        ChatRequest request,
        bool succeeded,
        long durationMs)
    {
        auditLogger.Record(new AuditEventRequest(
            "chat",
            action,
            route,
            succeeded,
            durationMs,
            new Dictionary<string, string>
            {
                ["documentId"] = request.DocumentId ?? string.Empty,
                ["historyCount"] = (request.History?.Count ?? 0).ToString(),
                ["aiProvider"] = request.AiProvider ?? string.Empty
            }));
    }

    private void RecordSafetyAudit(
        ChatRequest request,
        SafetyClassification classification)
    {
        // Safety classifier decisions are audited separately from normal chat completion.
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

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength
            ? value
            : $"{value[..maxLength]}...";
    }

    private static IReadOnlyList<string> BuildSummarySuggestedActions(ChatRequest request)
    {
        // Suggested actions are generated by the application when a skill result is adapted to chat.
        // Keep them in the user's language so clicking a suggestion feels like a user command.
        return PrefersChinese(request)
            ?
            [
                "\u5206\u6790\u98ce\u9669",
                "\u751f\u6210\u540e\u7eed\u90ae\u4ef6",
                "\u68c0\u67e5\u7b80\u5386\u5b9a\u4f4d"
            ]
            :
            [
                "Analyze risks",
                "Generate follow-up email",
                "Review resume positioning"
            ];
    }

    private static IReadOnlyList<string> BuildRiskSuggestedActions(ChatRequest request)
    {
        // Risk-analysis chat routes use app-generated suggestions, so they must also follow the user's language.
        return PrefersChinese(request)
            ?
            [
                "\u603b\u7ed3\u5173\u952e\u70b9",
                "\u751f\u6210\u540e\u7eed\u90ae\u4ef6",
                "\u6267\u884c\u5b8c\u6574\u6d41\u7a0b"
            ]
            :
            [
                "Summarize key points",
                "Generate follow-up email",
                "Run full workflow"
            ];
    }

    private static string BuildNoRisksMessage(ChatRequest request)
    {
        return PrefersChinese(request)
            ? "\u4ece\u5f53\u524d\u6587\u6863\u4e0a\u4e0b\u6587\u4e2d\u6ca1\u6709\u8bc6\u522b\u51fa\u660e\u663e\u98ce\u9669\u9879\u3002"
            : "No major risk items were identified from the selected document.";
    }

    private static string FormatSeverity(string severity, ChatRequest request)
    {
        if (!PrefersChinese(request))
        {
            return severity;
        }

        return severity.ToLowerInvariant() switch
        {
            "high" => "\u9ad8",
            "low" => "\u4f4e",
            _ => "\u4e2d"
        };
    }

    private static bool PrefersChinese(ChatRequest request)
    {
        return request.Message.Any(character => character is >= '\u4e00' and <= '\u9fff');
    }
}
