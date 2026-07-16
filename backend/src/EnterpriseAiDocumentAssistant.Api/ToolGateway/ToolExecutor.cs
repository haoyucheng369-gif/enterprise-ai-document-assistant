namespace EnterpriseAiDocumentAssistant.Api.ToolGateway;

public sealed class ToolExecutor : IToolExecutor
{
    private readonly IToolRegistry toolRegistry;

    public ToolExecutor(IToolRegistry toolRegistry)
    {
        this.toolRegistry = toolRegistry;
    }

    public Task<ToolExecutionResult> ExecuteAsync(
        ToolExecutionRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ToolName))
        {
            return Task.FromResult(new ToolExecutionResult(
                request.ToolName,
                false,
                "ToolName is required.",
                new Dictionary<string, object?>()));
        }

        if (!toolRegistry.TryGetTool(request.ToolName, out var tool) || tool is null)
        {
            return Task.FromResult(new ToolExecutionResult(
                request.ToolName,
                false,
                $"Tool '{request.ToolName}' is not registered.",
                new Dictionary<string, object?>()));
        }

        return tool.ExecuteAsync(request, cancellationToken);
    }
}
