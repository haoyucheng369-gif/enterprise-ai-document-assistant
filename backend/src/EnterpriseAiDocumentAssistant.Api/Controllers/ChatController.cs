using System.Diagnostics;
using EnterpriseAiDocumentAssistant.Api.Audit;
using EnterpriseAiDocumentAssistant.Api.AiGateway;
using EnterpriseAiDocumentAssistant.Api.Chat;
using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.Conversations;
using EnterpriseAiDocumentAssistant.Api.RateLimiting;
using EnterpriseAiDocumentAssistant.Api.Security;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiDocumentAssistant.Api.Controllers;

[ApiController]
[Route("api/chat")]
[EnableRateLimiting(AiRateLimitPolicy.Name)]
public sealed class ChatController : ControllerBase
{
    private readonly IChatOrchestrationService chatOrchestrationService;
    private readonly IAiGateway aiGateway;
    private readonly IAuditLogger auditLogger;
    private readonly IConversationRepository conversationRepository;
    private readonly IDocumentAccessPolicy documentAccessPolicy;

    public ChatController(
        IChatOrchestrationService chatOrchestrationService,
        IAiGateway aiGateway,
        IAuditLogger auditLogger,
        IConversationRepository conversationRepository,
        IDocumentAccessPolicy documentAccessPolicy)
    {
        this.chatOrchestrationService = chatOrchestrationService;
        this.aiGateway = aiGateway;
        this.auditLogger = auditLogger;
        this.conversationRepository = conversationRepository;
        this.documentAccessPolicy = documentAccessPolicy;
    }

    [HttpPost]
    [ProducesResponseType<ChatResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ChatResponse>> Post(
        ChatRequest request,
        CancellationToken cancellationToken)
    {
        // HTTP chat endpoint validates transport input, delegates orchestration, then wraps the UI response.
        var stopwatch = Stopwatch.StartNew();
        if (RequestHasMissingMessage(request))
        {
            return ValidationProblem(ModelState);
        }

        var accessProblem = BuildDocumentAccessProblem(request.DocumentId);
        if (accessProblem is not null)
        {
            return accessProblem;
        }

        var result = await chatOrchestrationService.BuildValidatedMessageAsync(request, cancellationToken);
        if (!result.IsValid || result.Message is null)
        {
            return StructuredOutputProblem(result);
        }

        var userMessage = new MessageResponse(
            $"user-{Guid.NewGuid():N}",
            "user",
            request.Message);
        var response = new MessageResponse(
            $"assistant-{Guid.NewGuid():N}",
            "assistant",
            result.Message.Answer,
            result.Message.Confidence,
            result.Message.Citations,
            result.Message.SuggestedActions);

        // Persist only complete, validated turns returned by the primary frontend endpoint.
        await conversationRepository.AppendTurnAsync(
            request.DocumentId,
            userMessage,
            response,
            cancellationToken);

        RecordChatAudit("chat_completed", "api/chat", request, true, stopwatch.ElapsedMilliseconds);
        return Ok(new ChatResponse(response, result.Message));
    }

    [HttpPost("structured")]
    [ProducesResponseType<StructuredChatResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<StructuredChatResponse>> Structured(
        ChatRequest request,
        CancellationToken cancellationToken)
    {
        // Structured endpoint exposes the validated assistant payload for Swagger and debugging.
        var stopwatch = Stopwatch.StartNew();
        if (RequestHasMissingMessage(request))
        {
            return ValidationProblem(ModelState);
        }

        var accessProblem = BuildDocumentAccessProblem(request.DocumentId);
        if (accessProblem is not null)
        {
            return accessProblem;
        }

        var result = await chatOrchestrationService.BuildValidatedMessageAsync(request, cancellationToken);
        if (!result.IsValid || result.Message is null)
        {
            return StructuredOutputProblem(result);
        }

        RecordChatAudit("structured_chat_completed", "api/chat/structured", request, true, stopwatch.ElapsedMilliseconds);
        return Ok(new StructuredChatResponse(result.Message));
    }

    [HttpPost("stream")]
    [Produces("text/plain")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Stream(ChatRequest request, CancellationToken cancellationToken)
    {
        // V1 streaming writes chunks from a complete structured response; native model streaming can replace this later.
        var stopwatch = Stopwatch.StartNew();
        if (RequestHasMissingMessage(request))
        {
            return ValidationProblem(ModelState);
        }

        var accessProblem = BuildDocumentAccessProblem(request.DocumentId);
        if (accessProblem is not null)
        {
            return accessProblem;
        }

        Response.ContentType = "text/plain; charset=utf-8";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Append("X-Accel-Buffering", "no");

        var result = await chatOrchestrationService.BuildValidatedMessageAsync(request, cancellationToken);
        if (!result.IsValid || result.Message is null)
        {
            return StructuredOutputProblem(result);
        }

        foreach (var chunk in aiGateway.BuildResponseChunks(result.Message))
        {
            await Response.WriteAsync(chunk, cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
            await Task.Delay(120, cancellationToken);
        }

        RecordChatAudit("stream_chat_completed", "api/chat/stream", request, true, stopwatch.ElapsedMilliseconds);
        return new EmptyResult();
    }

    private bool RequestHasMissingMessage(ChatRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Message))
        {
            return false;
        }

        ModelState.AddModelError(nameof(request.Message), "Message is required.");
        return true;
    }

    private ObjectResult StructuredOutputProblem(ChatOrchestrationResult result)
    {
        return Problem(
            title: "StructuredOutputValidationFailed",
            detail: string.Join(" ", result.Errors),
            statusCode: StatusCodes.Status502BadGateway);
    }

    private ObjectResult? BuildDocumentAccessProblem(string? documentId)
    {
        if (string.IsNullOrWhiteSpace(documentId)
            || documentAccessPolicy.Evaluate(documentId) != DocumentAccessLevel.Denied)
        {
            return null;
        }

        // Authorization is decided before guardrails, retrieval, tools, or model execution.
        return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
        {
            Title = "DocumentAccessDenied",
            Detail = "The current user is not allowed to access the selected document.",
            Status = StatusCodes.Status403Forbidden
        });
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
}
