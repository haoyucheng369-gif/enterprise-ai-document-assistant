using System.Text.Json;
using EnterpriseAiDocumentAssistant.Api.ToolGateway;

namespace EnterpriseAiDocumentAssistant.Api.Mcp;

public static class McpToolMapper
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static McpToolDescriptor ToMcpDescriptor(ToolDefinition definition)
    {
        // MCP list exposes the same internal tool definition as a client-readable input schema.
        var properties = definition.Parameters.ToDictionary(
            parameter => parameter.Key,
            parameter => new McpInputSchemaProperty(
                parameter.Value.Type,
                parameter.Value.Description),
            StringComparer.OrdinalIgnoreCase);

        var required = definition.Parameters
            .Where(parameter => parameter.Value.IsRequired)
            .Select(parameter => parameter.Key)
            .ToArray();

        return new McpToolDescriptor(
            definition.Name,
            definition.Description,
            new McpInputSchema("object", properties, required));
    }

    public static McpToolCallResponse ToMcpResponse(ToolExecutionResult result)
    {
        // MCP call wraps the internal tool result into the content/data shape external clients expect.
        var text = result.Succeeded
            ? JsonSerializer.Serialize(result.Data, SerializerOptions)
            : result.Error ?? "Tool execution failed.";

        return new McpToolCallResponse(
            IsError: !result.Succeeded,
            Content: [new McpContentItem("text", text)],
            Data: result.Data);
    }
}
