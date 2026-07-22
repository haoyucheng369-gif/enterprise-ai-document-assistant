using System.Diagnostics;
using EnterpriseAiDocumentAssistant.Api.Audit;
using EnterpriseAiDocumentAssistant.Api.AiGateway;
using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.Guardrails;
using EnterpriseAiDocumentAssistant.Api.PromptOrchestration;
using EnterpriseAiDocumentAssistant.Api.Services;
using EnterpriseAiDocumentAssistant.Api.StructuredOutput;
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

    public ChatController(
        IDocumentAssistantPromptOrchestrator promptOrchestrator,
        IAiGateway aiGateway,
        IStructuredAssistantResponseValidator structuredResponseValidator,
        IChatGuardrailEvaluator chatGuardrailEvaluator,
        IAuditLogger auditLogger,
        IApplicationDocumentProvider applicationDocumentProvider)
    {
        this.promptOrchestrator = promptOrchestrator;
        this.aiGateway = aiGateway;
        this.structuredResponseValidator = structuredResponseValidator;
        this.chatGuardrailEvaluator = chatGuardrailEvaluator;
        this.auditLogger = auditLogger;
        this.applicationDocumentProvider = applicationDocumentProvider;
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

        var prompt = promptOrchestrator.BuildPrompt(request);
        var modelResponse = await aiGateway.GenerateChatResponseAsync(
            new ChatModelRequest(prompt, request.AiProvider),
            cancellationToken);

        return ValidateStructuredMessage(AttachDocumentCitations(request, modelResponse.Message));
    }

    private StructuredAssistantMessage AttachDocumentCitations(
        ChatRequest request,
        StructuredAssistantMessage message)
    {
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
