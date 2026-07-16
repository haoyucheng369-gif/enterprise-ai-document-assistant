using EnterpriseAiDocumentAssistant.Api.Services;

namespace EnterpriseAiDocumentAssistant.Api.ToolGateway.Tools;

public sealed class GetHealthStatusTool : ITool
{
    private readonly IApiStatusProvider apiStatusProvider;

    public GetHealthStatusTool(IApiStatusProvider apiStatusProvider)
    {
        this.apiStatusProvider = apiStatusProvider;
    }

    public ToolDefinition Definition { get; } = new(
        "get_health_status",
        "Returns API status information including environment, version, provider, and current UTC time.",
        new Dictionary<string, ToolParameterDefinition>());

    public Task<ToolExecutionResult> ExecuteAsync(
        ToolExecutionRequest request,
        CancellationToken cancellationToken)
    {
        var status = apiStatusProvider.GetStatus();

        return Task.FromResult(new ToolExecutionResult(
            Definition.Name,
            true,
            null,
            new Dictionary<string, object?>
            {
                ["service"] = status.Service,
                ["environment"] = status.Environment,
                ["apiVersion"] = status.ApiVersion,
                ["version"] = status.Version,
                ["aiProvider"] = status.AiProvider,
                ["timeUtc"] = status.TimeUtc
            }));
    }
}
