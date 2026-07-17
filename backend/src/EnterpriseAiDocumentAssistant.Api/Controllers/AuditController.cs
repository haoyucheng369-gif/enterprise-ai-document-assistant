using EnterpriseAiDocumentAssistant.Api.Audit;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiDocumentAssistant.Api.Controllers;

[ApiController]
[Route("api/audit")]
public sealed class AuditController : ControllerBase
{
    private readonly IAuditLogger auditLogger;

    public AuditController(IAuditLogger auditLogger)
    {
        this.auditLogger = auditLogger;
    }

    [HttpGet("events")]
    [ProducesResponseType<IReadOnlyList<AuditEvent>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<AuditEvent>> ListEvents([FromQuery] int limit = 50)
    {
        return Ok(auditLogger.ListRecent(limit));
    }
}
