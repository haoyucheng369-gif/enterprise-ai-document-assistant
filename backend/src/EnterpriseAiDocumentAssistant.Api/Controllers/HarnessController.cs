using EnterpriseAiDocumentAssistant.Api.Harness;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiDocumentAssistant.Api.Controllers;

[ApiController]
[Route("api/harness")]
public sealed class HarnessController : ControllerBase
{
    private readonly IHarnessRunner harnessRunner;

    public HarnessController(IHarnessRunner harnessRunner)
    {
        this.harnessRunner = harnessRunner;
    }

    [HttpGet]
    [ProducesResponseType<HarnessReport>(StatusCodes.Status200OK)]
    public async Task<ActionResult<HarnessReport>> Run(CancellationToken cancellationToken)
    {
        return Ok(await harnessRunner.RunAsync(cancellationToken));
    }
}
