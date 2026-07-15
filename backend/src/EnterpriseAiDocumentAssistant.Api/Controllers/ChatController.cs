using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.PromptOrchestration;
using EnterpriseAiDocumentAssistant.Api.StructuredOutput;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiDocumentAssistant.Api.Controllers;

[ApiController]
[Route("api/chat")]
public sealed class ChatController : ControllerBase
{
    private readonly IDocumentAssistantPromptOrchestrator promptOrchestrator;
    private readonly IStructuredAssistantResponseValidator structuredResponseValidator;

    public ChatController(
        IDocumentAssistantPromptOrchestrator promptOrchestrator,
        IStructuredAssistantResponseValidator structuredResponseValidator)
    {
        this.promptOrchestrator = promptOrchestrator;
        this.structuredResponseValidator = structuredResponseValidator;
    }

    [HttpPost]
    [ProducesResponseType<ChatResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<ChatResponse> Post(ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            ModelState.AddModelError(nameof(request.Message), "Message is required.");
            return ValidationProblem(ModelState);
        }

        var structuredMessage = BuildValidatedStructuredMessage(request);
        if (structuredMessage.Result is not null)
        {
            return structuredMessage.Result;
        }

        var message = structuredMessage.Value
            ?? throw new InvalidOperationException("Structured message was not created.");
        var responseContent = string.Concat(
            promptOrchestrator.BuildMockResponseChunks(message));

        var response = new MessageResponse(
            $"assistant-{Guid.NewGuid():N}",
            "assistant",
            responseContent);

        return Ok(new ChatResponse(response));
    }

    [HttpPost("structured")]
    [ProducesResponseType<StructuredChatResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<StructuredChatResponse> Structured(ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            ModelState.AddModelError(nameof(request.Message), "Message is required.");
            return ValidationProblem(ModelState);
        }

        var structuredMessage = BuildValidatedStructuredMessage(request);
        if (structuredMessage.Result is not null)
        {
            return structuredMessage.Result;
        }

        var message = structuredMessage.Value
            ?? throw new InvalidOperationException("Structured message was not created.");

        return Ok(new StructuredChatResponse(message));
    }

    [HttpPost("stream")]
    [Produces("text/plain")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Stream(ChatRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            ModelState.AddModelError(nameof(request.Message), "Message is required.");
            return ValidationProblem(ModelState);
        }

        Response.ContentType = "text/plain; charset=utf-8";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Append("X-Accel-Buffering", "no");

        var structuredMessage = BuildValidatedStructuredMessage(request);
        if (structuredMessage.Result is not null)
        {
            return structuredMessage.Result;
        }

        var message = structuredMessage.Value
            ?? throw new InvalidOperationException("Structured message was not created.");

        foreach (var chunk in promptOrchestrator.BuildMockResponseChunks(message))
        {
            await Response.WriteAsync(chunk, cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
            await Task.Delay(120, cancellationToken);
        }

        return new EmptyResult();
    }

    private ActionResult<StructuredAssistantMessage> BuildValidatedStructuredMessage(
        ChatRequest request)
    {
        var prompt = promptOrchestrator.BuildPrompt(request);
        var structuredMessage = promptOrchestrator.BuildMockStructuredResponse(prompt);
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
}
