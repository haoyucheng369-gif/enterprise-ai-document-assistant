namespace EnterpriseAiDocumentAssistant.Api.ToolGateway;

public interface ITool
{
    ToolDefinition Definition { get; }

    Task<ToolExecutionResult> ExecuteAsync(
        ToolExecutionRequest request,
        CancellationToken cancellationToken);
}
