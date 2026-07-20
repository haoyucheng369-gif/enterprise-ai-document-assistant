using System.Text.Json;

namespace EnterpriseAiDocumentAssistant.Api.ToolGateway;

public sealed record ToolExecutionRequest(
    string ToolName,
    IReadOnlyDictionary<string, JsonElement> Arguments);

public sealed record ToolExecutionResult(
    string ToolName,
    bool Succeeded,
    string? Error,
    IReadOnlyDictionary<string, object?> Data);

public sealed record ToolListResponse(
    IReadOnlyList<ToolDefinition> Tools);
