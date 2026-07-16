using EnterpriseAiDocumentAssistant.Api.ToolGateway;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiDocumentAssistant.Api.Controllers;

[ApiController]
[Route("api/tools")]
public sealed class ToolsController : ControllerBase
{
    private readonly IToolRegistry toolRegistry;
    private readonly IToolExecutor toolExecutor;

    public ToolsController(
        IToolRegistry toolRegistry,
        IToolExecutor toolExecutor)
    {
        this.toolRegistry = toolRegistry;
        this.toolExecutor = toolExecutor;
    }

    [HttpGet]
    [ProducesResponseType<ToolListResponse>(StatusCodes.Status200OK)]
    public ActionResult<ToolListResponse> List()
    {
        return Ok(new ToolListResponse(toolRegistry.ListDefinitions()));
    }

    [HttpPost("execute")]
    [ProducesResponseType<ToolExecutionResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ToolExecutionResult>> Execute(
        ToolExecutionRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ToolName))
        {
            ModelState.AddModelError(nameof(request.ToolName), "ToolName is required.");
            return ValidationProblem(ModelState);
        }

        var result = await toolExecutor.ExecuteAsync(request, cancellationToken);

        if (!result.Succeeded && result.Error?.Contains("not registered", StringComparison.OrdinalIgnoreCase) == true)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "ToolNotFound",
                Detail = result.Error
            });
        }

        return Ok(result);
    }
}
