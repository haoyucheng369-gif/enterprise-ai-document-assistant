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
}
