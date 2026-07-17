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

    public SkillsController(
        ISummarySkill summarySkill,
        IRiskAnalysisSkill riskAnalysisSkill,
        IEmailDraftSkill emailDraftSkill)
    {
        this.summarySkill = summarySkill;
        this.riskAnalysisSkill = riskAnalysisSkill;
        this.emailDraftSkill = emailDraftSkill;
    }

    [HttpPost("summary")]
    [ProducesResponseType<SummarySkillResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<SummarySkillResponse> Summarize(SummarySkillRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DocumentId))
        {
            ModelState.AddModelError(nameof(request.DocumentId), "DocumentId is required.");
            return ValidationProblem(ModelState);
        }

        var result = summarySkill.Run(request);

        if (result is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "DocumentNotFound",
                Detail = $"Document '{request.DocumentId}' was not found."
            });
        }

        return Ok(result);
    }

    [HttpPost("risk-analysis")]
    [ProducesResponseType<RiskAnalysisSkillResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<RiskAnalysisSkillResponse> AnalyzeRisks(RiskAnalysisSkillRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DocumentId))
        {
            ModelState.AddModelError(nameof(request.DocumentId), "DocumentId is required.");
            return ValidationProblem(ModelState);
        }

        var result = riskAnalysisSkill.Run(request);

        if (result is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "DocumentNotFound",
                Detail = $"Document '{request.DocumentId}' was not found."
            });
        }

        return Ok(result);
    }

    [HttpPost("email-draft")]
    [ProducesResponseType<EmailDraftSkillResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<EmailDraftSkillResponse> DraftEmail(EmailDraftSkillRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DocumentId))
        {
            ModelState.AddModelError(nameof(request.DocumentId), "DocumentId is required.");
            return ValidationProblem(ModelState);
        }

        var result = emailDraftSkill.Run(request);

        if (result is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "DocumentNotFound",
                Detail = $"Document '{request.DocumentId}' was not found."
            });
        }

        return Ok(result);
    }
}
