using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiDocumentAssistant.Api.Controllers;

[ApiController]
[Route("api/status")]
public sealed class StatusController : ControllerBase
{
    private readonly IApiStatusProvider apiStatusProvider;

    public StatusController(IApiStatusProvider apiStatusProvider)
    {
        this.apiStatusProvider = apiStatusProvider;
    }

    [HttpGet]
    [ProducesResponseType<StatusResponse>(StatusCodes.Status200OK)]
    public ActionResult<StatusResponse> Get()
    {
        // Small status endpoint used by health-style tools and frontend diagnostics.
        return Ok(apiStatusProvider.GetStatus());
    }
}
