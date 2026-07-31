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
        // Swagger/debug endpoint for recent in-memory audit events.
        return Ok(auditLogger.ListRecent(limit));
    }

    [HttpGet("ai-executions")]
    [ProducesResponseType<IReadOnlyList<AiExecutionResponse>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<AiExecutionResponse>> ListAiExecutions([FromQuery] int limit = 50)
    {
        // Present AI-specific audit metadata as a small observability view without exposing prompt content.
        var boundedLimit = Math.Clamp(limit, 1, 100);
        var executions = auditLogger
            .ListRecent(int.MaxValue)
            .Where(item => string.Equals(item.Category, "ai_gateway", StringComparison.Ordinal))
            .Take(boundedLimit)
            .Select(ToAiExecution)
            .ToArray();

        return Ok(executions);
    }

    private static AiExecutionResponse ToAiExecution(AuditEvent auditEvent)
    {
        return new AiExecutionResponse(
            auditEvent.Id,
            auditEvent.Timestamp,
            auditEvent.Action,
            auditEvent.Route,
            ReadMetadata(auditEvent, "model"),
            ReadMetadata(auditEvent, "userId", "unknown"),
            auditEvent.Succeeded,
            auditEvent.DurationMs,
            ReadIntMetadata(auditEvent, "inputTokenEstimate"),
            ReadIntMetadata(auditEvent, "outputTokenEstimate"));
    }

    private static string ReadMetadata(
        AuditEvent auditEvent,
        string name,
        string fallback = "unknown")
    {
        return auditEvent.Metadata.TryGetValue(name, out var value)
            && !string.IsNullOrWhiteSpace(value)
                ? value
                : fallback;
    }

    private static int? ReadIntMetadata(AuditEvent auditEvent, string name)
    {
        return auditEvent.Metadata.TryGetValue(name, out var value)
            && int.TryParse(value, out var parsed)
                ? parsed
                : null;
    }
}
