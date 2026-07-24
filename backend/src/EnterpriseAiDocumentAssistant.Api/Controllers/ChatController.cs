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
        var response = new MessageResponse(
            $"assistant-{Guid.NewGuid():N}",
            "assistant",
            message.Answer);

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
        var guardrailEvaluation = chatGuardrailEvaluator.Evaluate(request);
        if (guardrailEvaluation.IsBlocked)
        {
            return ValidateStructuredMessage(guardrailEvaluation.Response
                ?? throw new InvalidOperationException("Guardrail response was not created."));
        }

        // Planner decides whether this free-form chat message should stay as chat or be routed to a known capability.
        var plan = await agentPlanner.PlanAsync(
            new AgentPlanRequest(request.Message, request.DocumentId, request.AiProvider),
            cancellationToken);

        // If the route maps to a skill/workflow, execute it now and adapt its result back to the chat response shape.
        var plannedMessage = await TryExecutePlannedRouteAsync(request, plan, cancellationToken);
        if (plannedMessage is not null)
        {
            return ValidateStructuredMessage(AttachDocumentCitations(request, plannedMessage));
        }

        // No specialized route was selected, so the request continues through the normal assistant prompt.
        var prompt = promptOrchestrator.BuildAssistantPrompt(request);
        var modelResponse = await aiGateway.GenerateChatResponseAsync(
            new ChatModelRequest(prompt, request.AiProvider),
            cancellationToken);

        return ValidateStructuredMessage(AttachDocumentCitations(request, modelResponse.Message));
    }

    private async Task<StructuredAssistantMessage?> TryExecutePlannedRouteAsync(
        ChatRequest request,
        AgentPlanResponse plan,
        CancellationToken cancellationToken)
    {
        // This switch is where planner output becomes real application behavior.
        // Each route calls one skill/workflow, then the result is normalized for the Assistant UI.
        return plan.Route switch
        {
            "skills.summary" => ConvertSummaryToAssistantMessage(await summarySkill.RunAsync(
                new SummarySkillRequest(plan.DocumentId, request.AiProvider),
                cancellationToken)),
            "skills.risk-analysis" => ConvertRiskAnalysisToAssistantMessage(await riskAnalysisSkill.RunAsync(
                new RiskAnalysisSkillRequest(plan.DocumentId, request.AiProvider),
                cancellationToken)),
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

    private static StructuredAssistantMessage? ConvertSummaryToAssistantMessage(SummarySkillResponse? response)
    {
        return response is null
            ? null
            : new StructuredAssistantMessage(
                response.Summary,
                "high",
                response.Sources,
                ["Analyze risks", "Generate follow-up email", "Review resume positioning"]);
    }

    private static StructuredAssistantMessage? ConvertRiskAnalysisToAssistantMessage(RiskAnalysisSkillResponse? response)
    {
        if (response is null)
        {
            return null;
        }

        var answer = response.Risks.Count == 0
            ? "No major risk items were identified from the selected document."
            : string.Join(Environment.NewLine, response.Risks.Select(risk =>
                $"- {risk.Title} ({risk.Severity}): {risk.Recommendation}"));

        return new StructuredAssistantMessage(
            answer,
            "high",
            response.Risks.Select(risk => risk.Source).ToArray(),
            ["Summarize key points", "Generate follow-up email", "Run full workflow"]);
    }

    private static StructuredAssistantMessage? ConvertEmailDraftToAssistantMessage(EmailDraftSkillResponse? response)
    {
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

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength
            ? value
            : $"{value[..maxLength]}...";
    }
}
