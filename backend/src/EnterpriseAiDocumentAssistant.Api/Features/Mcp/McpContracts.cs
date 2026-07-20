using System.Text.Json;

namespace EnterpriseAiDocumentAssistant.Api.Mcp;

public sealed record McpToolListResponse(
    IReadOnlyList<McpToolDescriptor> Tools);

public sealed record McpToolDescriptor(
    string Name,
    string Description,
    McpInputSchema InputSchema);

public sealed record McpInputSchema(
    string Type,
    IReadOnlyDictionary<string, McpInputSchemaProperty> Properties,
    IReadOnlyList<string> Required);

public sealed record McpInputSchemaProperty(
    string Type,
    string Description);

public sealed record McpToolCallRequest(
    string Name,
    IReadOnlyDictionary<string, JsonElement>? Arguments);

public sealed record McpToolCallResponse(
    bool IsError,
    IReadOnlyList<McpContentItem> Content,
    IReadOnlyDictionary<string, object?> Data);

public sealed record McpContentItem(
    string Type,
    string Text);
