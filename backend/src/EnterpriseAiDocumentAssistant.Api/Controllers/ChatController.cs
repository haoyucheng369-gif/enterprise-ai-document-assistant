using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.PromptOrchestration;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiDocumentAssistant.Api.Controllers;

[ApiController]
[Route("api/chat")]
public sealed class ChatController : ControllerBase
{
    private readonly IDocumentAssistantPromptOrchestrator promptOrchestrator;

    public ChatController(IDocumentAssistantPromptOrchestrator promptOrchestrator)
    {
        this.promptOrchestrator = promptOrchestrator;
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

        var prompt = promptOrchestrator.BuildPrompt(request);
        var responseContent = string.Concat(promptOrchestrator.BuildMockResponseChunks(prompt));

        var response = new MessageResponse(
            $"assistant-{Guid.NewGuid():N}",
            "assistant",
            responseContent);

        return Ok(new ChatResponse(response));
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

        var prompt = promptOrchestrator.BuildPrompt(request);

        foreach (var chunk in promptOrchestrator.BuildMockResponseChunks(prompt))
        {
            await Response.WriteAsync(chunk, cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
            await Task.Delay(120, cancellationToken);
        }

        return new EmptyResult();
    }
}
