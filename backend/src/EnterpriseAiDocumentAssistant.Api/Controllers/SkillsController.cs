using EnterpriseAiDocumentAssistant.Api.Skills;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiDocumentAssistant.Api.Controllers;

[ApiController]
[Route("api/skills")]
public sealed class SkillsController : ControllerBase
{
    private readonly ISummarySkill summarySkill;

    public SkillsController(ISummarySkill summarySkill)
    {
        this.summarySkill = summarySkill;
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
}
