using System.Diagnostics;
using EnterpriseAiDocumentAssistant.Api.Audit;
using EnterpriseAiDocumentAssistant.Api.Skills;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiDocumentAssistant.Api.Controllers;

[ApiController]
[Route("api/skills")]
public sealed class SkillsController : ControllerBase
{
    private readonly ISummarySkill summarySkill;
    private readonly IRiskAnalysisSkill riskAnalysisSkill;
    private readonly IEmailDraftSkill emailDraftSkill;
    private readonly IAuditLogger auditLogger;

    public SkillsController(
        ISummarySkill summarySkill,
        IRiskAnalysisSkill riskAnalysisSkill,
        IEmailDraftSkill emailDraftSkill,
        IAuditLogger auditLogger)
    {
        this.summarySkill = summarySkill;
        this.riskAnalysisSkill = riskAnalysisSkill;
        this.emailDraftSkill = emailDraftSkill;
        this.auditLogger = auditLogger;
    }

    [HttpPost("summary")]
    [ProducesResponseType<SummarySkillResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<SummarySkillResponse> Summarize(SummarySkillRequest request)
    {
        var stopwatch = Stopwatch.StartNew();

        if (string.IsNullOrWhiteSpace(request.DocumentId))
        {
            ModelState.AddModelError(nameof(request.DocumentId), "DocumentId is required.");
            return ValidationProblem(ModelState);
        }

        var result = summarySkill.Run(request);

        if (result is null)
        {
            RecordSkillAudit("summary", "skills.summary", request.DocumentId, false, stopwatch.ElapsedMilliseconds);
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "DocumentNotFound",
                Detail = $"Document '{request.DocumentId}' was not found."
            });
        }

        RecordSkillAudit("summary", "skills.summary", request.DocumentId, true, stopwatch.ElapsedMilliseconds);
        return Ok(result);
    }

    [HttpPost("risk-analysis")]
    [ProducesResponseType<RiskAnalysisSkillResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<RiskAnalysisSkillResponse> AnalyzeRisks(RiskAnalysisSkillRequest request)
    {
        var stopwatch = Stopwatch.StartNew();

        if (string.IsNullOrWhiteSpace(request.DocumentId))
        {
            ModelState.AddModelError(nameof(request.DocumentId), "DocumentId is required.");
            return ValidationProblem(ModelState);
        }

        var result = riskAnalysisSkill.Run(request);

        if (result is null)
        {
            RecordSkillAudit("risk_analysis", "skills.risk-analysis", request.DocumentId, false, stopwatch.ElapsedMilliseconds);
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "DocumentNotFound",
                Detail = $"Document '{request.DocumentId}' was not found."
            });
        }

        RecordSkillAudit("risk_analysis", "skills.risk-analysis", request.DocumentId, true, stopwatch.ElapsedMilliseconds);
        return Ok(result);
    }

    [HttpPost("email-draft")]
    [ProducesResponseType<EmailDraftSkillResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<EmailDraftSkillResponse> DraftEmail(EmailDraftSkillRequest request)
    {
        var stopwatch = Stopwatch.StartNew();

        if (string.IsNullOrWhiteSpace(request.DocumentId))
        {
            ModelState.AddModelError(nameof(request.DocumentId), "DocumentId is required.");
            return ValidationProblem(ModelState);
        }

        var result = emailDraftSkill.Run(request);

        if (result is null)
        {
            RecordSkillAudit("email_draft", "skills.email-draft", request.DocumentId, false, stopwatch.ElapsedMilliseconds);
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "DocumentNotFound",
                Detail = $"Document '{request.DocumentId}' was not found."
            });
        }

        RecordSkillAudit("email_draft", "skills.email-draft", request.DocumentId, true, stopwatch.ElapsedMilliseconds);
        return Ok(result);
    }

    private void RecordSkillAudit(
        string action,
        string route,
        string documentId,
        bool succeeded,
        long durationMs)
    {
        auditLogger.Record(new AuditEventRequest(
            "skill",
            action,
            route,
            succeeded,
            durationMs,
            new Dictionary<string, string>
            {
                ["documentId"] = documentId
            }));
    }
}
