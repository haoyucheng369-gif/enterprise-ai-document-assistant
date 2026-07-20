namespace EnterpriseAiDocumentAssistant.Api.ToolGateway;

public sealed record ToolDefinition(
    string Name,
    string Description,
    IReadOnlyDictionary<string, ToolParameterDefinition> Parameters);

public sealed record ToolParameterDefinition(
    string Type,
    string Description,
    bool IsRequired);
