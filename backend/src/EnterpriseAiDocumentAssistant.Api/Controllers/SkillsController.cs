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
    private readonly IClassificationSkill classificationSkill;
    private readonly IAuditLogger auditLogger;

    public SkillsController(
        ISummarySkill summarySkill,
        IRiskAnalysisSkill riskAnalysisSkill,
        IEmailDraftSkill emailDraftSkill,
        IClassificationSkill classificationSkill,
        IAuditLogger auditLogger)
    {
        this.summarySkill = summarySkill;
        this.riskAnalysisSkill = riskAnalysisSkill;
        this.emailDraftSkill = emailDraftSkill;
        this.classificationSkill = classificationSkill;
        this.auditLogger = auditLogger;
    }

    [HttpPost("summary")]
    [ProducesResponseType<SummarySkillResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SummarySkillResponse>> Summarize(
        SummarySkillRequest request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        var validationResult = ValidateDocumentId(request.DocumentId);
        if (validationResult is not null)
        {
            return validationResult;
        }

        var result = await summarySkill.RunAsync(request, cancellationToken);

        if (result is null)
        {
            RecordSkillAudit("summary", "skills.summary", request.DocumentId, false, stopwatch.ElapsedMilliseconds);
            return DocumentNotFound(request.DocumentId);
        }

        RecordSkillAudit("summary", "skills.summary", request.DocumentId, true, stopwatch.ElapsedMilliseconds);
        return Ok(result);
    }

    [HttpPost("risk-analysis")]
    [ProducesResponseType<RiskAnalysisSkillResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RiskAnalysisSkillResponse>> AnalyzeRisks(
        RiskAnalysisSkillRequest request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        var validationResult = ValidateDocumentId(request.DocumentId);
        if (validationResult is not null)
        {
            return validationResult;
        }

        var result = await riskAnalysisSkill.RunAsync(request, cancellationToken);

        if (result is null)
        {
            RecordSkillAudit("risk_analysis", "skills.risk-analysis", request.DocumentId, false, stopwatch.ElapsedMilliseconds);
            return DocumentNotFound(request.DocumentId);
        }

        RecordSkillAudit("risk_analysis", "skills.risk-analysis", request.DocumentId, true, stopwatch.ElapsedMilliseconds);
        return Ok(result);
    }

    [HttpPost("email-draft")]
    [ProducesResponseType<EmailDraftSkillResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmailDraftSkillResponse>> DraftEmail(
        EmailDraftSkillRequest request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        var validationResult = ValidateDocumentId(request.DocumentId);
        if (validationResult is not null)
        {
            return validationResult;
        }

        var result = await emailDraftSkill.RunAsync(request, cancellationToken);

        if (result is null)
        {
            RecordSkillAudit("email_draft", "skills.email-draft", request.DocumentId, false, stopwatch.ElapsedMilliseconds);
            return DocumentNotFound(request.DocumentId);
        }

        RecordSkillAudit("email_draft", "skills.email-draft", request.DocumentId, true, stopwatch.ElapsedMilliseconds);
        return Ok(result);
    }

    [HttpPost("classification")]
    [ProducesResponseType<ClassificationSkillResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClassificationSkillResponse>> Classify(
        ClassificationSkillRequest request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        var validationResult = ValidateDocumentId(request.DocumentId);
        if (validationResult is not null)
        {
            return validationResult;
        }

        var result = await classificationSkill.RunAsync(request, cancellationToken);

        if (result is null)
        {
            RecordSkillAudit("classification", "skills.classification", request.DocumentId, false, stopwatch.ElapsedMilliseconds);
            return DocumentNotFound(request.DocumentId);
        }

        RecordSkillAudit("classification", "skills.classification", request.DocumentId, true, stopwatch.ElapsedMilliseconds);
        return Ok(result);
    }

    private ActionResult? ValidateDocumentId(string documentId)
    {
        if (!string.IsNullOrWhiteSpace(documentId))
        {
            return null;
        }

        ModelState.AddModelError(nameof(documentId), "DocumentId is required.");
        return ValidationProblem(ModelState);
    }

    private NotFoundObjectResult DocumentNotFound(string documentId)
    {
        return NotFound(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "DocumentNotFound",
            Detail = $"Document '{documentId}' was not found."
        });
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
