namespace EnterpriseAiDocumentAssistant.Api.ToolGateway;

public interface IToolExecutor
{
    Task<ToolExecutionResult> ExecuteAsync(
        ToolExecutionRequest request,
        CancellationToken cancellationToken);
}
