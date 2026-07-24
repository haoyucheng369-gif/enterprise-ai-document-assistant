using EnterpriseAiDocumentAssistant.Api.Planner;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiDocumentAssistant.Api.Controllers;

[ApiController]
[Route("api/planner")]
public sealed class PlannerController : ControllerBase
{
    private readonly IAgentPlanner agentPlanner;

    public PlannerController(IAgentPlanner agentPlanner)
    {
        this.agentPlanner = agentPlanner;
    }

    [HttpPost("plan")]
    [ProducesResponseType<AgentPlanResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AgentPlanResponse>> Plan(
        AgentPlanRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            ModelState.AddModelError(nameof(request.Message), "Message is required.");
            return ValidationProblem(ModelState);
        }

        var plan = await agentPlanner.PlanAsync(request, cancellationToken);

        return Ok(plan);
    }
}
