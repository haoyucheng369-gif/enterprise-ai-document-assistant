using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.Options;
using EnterpriseAiDocumentAssistant.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EnterpriseAiDocumentAssistant.Api.Controllers;

[ApiController]
[Route("api/status")]
public sealed class StatusController : ControllerBase
{
    private const string ServiceName = "Enterprise AI Document Assistant API";
    private const string ApiVersion = "v1";
    private const string Version = "0.1.0";

    private readonly IHostEnvironment _environment;
    private readonly IOptions<AiGatewayOptions> _aiGatewayOptions;
    private readonly ISystemClock _clock;

    public StatusController(
        IHostEnvironment environment,
        IOptions<AiGatewayOptions> aiGatewayOptions,
        ISystemClock clock)
    {
        _environment = environment;
        _aiGatewayOptions = aiGatewayOptions;
        _clock = clock;
    }

    [HttpGet]
    [ProducesResponseType<StatusResponse>(StatusCodes.Status200OK)]
    public ActionResult<StatusResponse> Get()
    {
        return Ok(new StatusResponse(
            ServiceName,
            _environment.EnvironmentName,
            ApiVersion,
            Version,
            _aiGatewayOptions.Value.Provider,
            _clock.UtcNow));
    }
}
