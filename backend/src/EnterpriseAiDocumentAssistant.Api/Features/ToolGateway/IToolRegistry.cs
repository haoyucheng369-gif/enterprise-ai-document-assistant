namespace EnterpriseAiDocumentAssistant.Api.ToolGateway;

public interface IToolRegistry
{
    IReadOnlyList<ToolDefinition> ListDefinitions();

    bool TryGetTool(string toolName, out ITool? tool);
}
