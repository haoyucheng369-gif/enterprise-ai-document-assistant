namespace EnterpriseAiDocumentAssistant.Api.ToolGateway;

public sealed record ToolDefinition(
    string Name,
    string Description,
    IReadOnlyDictionary<string, ToolParameterDefinition> Parameters,
    bool IsReadOnly = true);

public sealed record ToolParameterDefinition(
    string Type,
    string Description,
    bool IsRequired);
