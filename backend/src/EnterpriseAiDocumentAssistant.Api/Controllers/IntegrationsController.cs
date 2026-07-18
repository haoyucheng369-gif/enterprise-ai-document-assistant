using EnterpriseAiDocumentAssistant.Api.Integrations.MicrosoftGraph;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiDocumentAssistant.Api.Controllers;

[ApiController]
[Route("api/integrations")]
public sealed class IntegrationsController : ControllerBase
{
    private readonly IMicrosoftGraphGateway microsoftGraphGateway;

    public IntegrationsController(IMicrosoftGraphGateway microsoftGraphGateway)
    {
        this.microsoftGraphGateway = microsoftGraphGateway;
    }

    [HttpPost("graph/email-draft")]
    [ProducesResponseType<MicrosoftGraphEmailDraftResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<MicrosoftGraphEmailDraftResponse> CreateGraphEmailDraft(
        MicrosoftGraphEmailDraftRequest request)
    {
        // Keep Graph-specific validation at the HTTP edge before calling the integration adapter.
        if (string.IsNullOrWhiteSpace(request.DocumentId))
        {
            ModelState.AddModelError(nameof(request.DocumentId), "DocumentId is required.");
        }

        if (string.IsNullOrWhiteSpace(request.To))
        {
            ModelState.AddModelError(nameof(request.To), "To is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Subject))
        {
            ModelState.AddModelError(nameof(request.Subject), "Subject is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            ModelState.AddModelError(nameof(request.Body), "Body is required.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        return Ok(microsoftGraphGateway.CreateEmailDraft(request));
    }
}
