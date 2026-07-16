using EnterpriseAiDocumentAssistant.Api.Mcp;
using EnterpriseAiDocumentAssistant.Api.ToolGateway;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiDocumentAssistant.Api.Controllers;

[ApiController]
[Route("api/mcp")]
public sealed class McpController : ControllerBase
{
    private readonly IToolRegistry toolRegistry;
    private readonly IToolExecutor toolExecutor;

    public McpController(
        IToolRegistry toolRegistry,
        IToolExecutor toolExecutor)
    {
        this.toolRegistry = toolRegistry;
        this.toolExecutor = toolExecutor;
    }

    [HttpGet("tools/list")]
    [ProducesResponseType<McpToolListResponse>(StatusCodes.Status200OK)]
    public ActionResult<McpToolListResponse> ListTools()
    {
        var tools = toolRegistry
            .ListDefinitions()
            .Select(McpToolMapper.ToMcpDescriptor)
            .ToArray();

        return Ok(new McpToolListResponse(tools));
    }

    [HttpPost("tools/call")]
    [ProducesResponseType<McpToolCallResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<McpToolCallResponse>> CallTool(
        McpToolCallRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            ModelState.AddModelError(nameof(request.Name), "Name is required.");
            return ValidationProblem(ModelState);
        }

        var toolRequest = new ToolExecutionRequest(
            request.Name,
            request.Arguments ?? new Dictionary<string, System.Text.Json.JsonElement>());

        var result = await toolExecutor.ExecuteAsync(toolRequest, cancellationToken);

        if (!result.Succeeded && result.Error?.Contains("not registered", StringComparison.OrdinalIgnoreCase) == true)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "McpToolNotFound",
                Detail = result.Error
            });
        }

        return Ok(McpToolMapper.ToMcpResponse(result));
    }
}
