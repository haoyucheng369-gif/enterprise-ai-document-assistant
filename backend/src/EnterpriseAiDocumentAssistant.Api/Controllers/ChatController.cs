using EnterpriseAiDocumentAssistant.Api.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiDocumentAssistant.Api.Controllers;

[ApiController]
[Route("api/chat")]
public sealed class ChatController : ControllerBase
{
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

        var trimmedMessage = request.Message.Trim();
        var documentContext = string.IsNullOrWhiteSpace(request.DocumentId)
            ? "the selected document"
            : $"document '{request.DocumentId}'";

        var response = new MessageResponse(
            $"assistant-{Guid.NewGuid():N}",
            "assistant",
            $"I received your question about {documentContext}: \"{trimmedMessage}\". This is a mock API response; model integration will be added in a later step.");

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

        foreach (var chunk in BuildMockResponseChunks(request))
        {
            await Response.WriteAsync(chunk, cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
            await Task.Delay(120, cancellationToken);
        }

        return new EmptyResult();
    }

    private static IEnumerable<string> BuildMockResponseChunks(ChatRequest request)
    {
        var trimmedMessage = request.Message.Trim();
        var documentContext = string.IsNullOrWhiteSpace(request.DocumentId)
            ? "the selected document"
            : $"document '{request.DocumentId}'";

        yield return $"I am reviewing {documentContext}. ";
        yield return $"Your question was: \"{trimmedMessage}\". ";
        yield return "This response is streamed from the ASP.NET Core API in small chunks. ";
        yield return "The next backend step can route the same flow through prompt orchestration and the AI Gateway.";
    }
}
