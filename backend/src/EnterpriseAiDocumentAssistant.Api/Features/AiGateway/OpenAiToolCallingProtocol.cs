using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EnterpriseAiDocumentAssistant.Api.Options;
using EnterpriseAiDocumentAssistant.Api.ToolGateway;

namespace EnterpriseAiDocumentAssistant.Api.AiGateway;

internal static class OpenAiToolCallingProtocol
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static HttpRequestMessage BuildRequest(
        AiGatewayOptions options,
        ToolSelectionModelRequest request,
        string provider)
    {
        // Convert internal Tool definitions into the OpenAI-compatible function schema.
        var endpoint = options.Endpoint.TrimEnd('/');
        var isAzureOpenAi = string.Equals(provider, "AzureOpenAI", StringComparison.OrdinalIgnoreCase);
        var requestUri = isAzureOpenAi
            ? $"{endpoint}/openai/deployments/{Uri.EscapeDataString(options.ChatModel)}/chat/completions?api-version={Uri.EscapeDataString(options.ApiVersion)}"
            : $"{endpoint}/v1/chat/completions";
        var payload = new Dictionary<string, object?>
        {
            ["messages"] = new object[]
            {
                new
                {
                    role = "system",
                    content = "Select at most one read-only tool when required. Do not invent tool names or arguments."
                },
                new
                {
                    role = "user",
                    content = $"""
                               User request:
                               {request.UserMessage}

                               Selected document id:
                               {request.DocumentId ?? "No document selected"}
                               """
                }
            },
            ["tools"] = request.Tools.Select(BuildProviderToolDefinition).ToArray(),
            ["tool_choice"] = "auto",
            ["temperature"] = 0
        };

        if (!isAzureOpenAi)
        {
            payload["model"] = options.ChatModel;
        }

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(payload, JsonOptions),
                Encoding.UTF8,
                "application/json")
        };

        if (isAzureOpenAi)
        {
            httpRequest.Headers.Add("api-key", options.ApiKey);
        }
        else
        {
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
        }

        return httpRequest;
    }

    public static ToolCallDecision? ParseResponse(string responseJson)
    {
        // Read the provider's first function call and clone arguments beyond JsonDocument lifetime.
        using var document = JsonDocument.Parse(responseJson);
        var message = document.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message");

        if (!message.TryGetProperty("tool_calls", out var toolCalls)
            || toolCalls.ValueKind != JsonValueKind.Array
            || toolCalls.GetArrayLength() == 0)
        {
            return null;
        }

        var function = toolCalls[0].GetProperty("function");
        var toolName = function.GetProperty("name").GetString();
        var argumentsJson = function.GetProperty("arguments").GetString();
        if (string.IsNullOrWhiteSpace(toolName))
        {
            return null;
        }

        var arguments = new Dictionary<string, JsonElement>();
        if (!string.IsNullOrWhiteSpace(argumentsJson))
        {
            using var argumentsDocument = JsonDocument.Parse(argumentsJson);
            if (argumentsDocument.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            foreach (var property in argumentsDocument.RootElement.EnumerateObject())
            {
                arguments[property.Name] = property.Value.Clone();
            }
        }

        return new ToolCallDecision(toolName, arguments);
    }

    private static object BuildProviderToolDefinition(ToolDefinition definition)
    {
        var properties = definition.Parameters.ToDictionary(
            parameter => parameter.Key,
            parameter => (object)new
            {
                type = parameter.Value.Type,
                description = parameter.Value.Description
            });
        var required = definition.Parameters
            .Where(parameter => parameter.Value.IsRequired)
            .Select(parameter => parameter.Key)
            .ToArray();

        return new
        {
            type = "function",
            function = new
            {
                name = definition.Name,
                description = definition.Description,
                parameters = new
                {
                    type = "object",
                    additionalProperties = false,
                    properties,
                    required
                }
            }
        };
    }
}
