using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EnterpriseAiDocumentAssistant.Api.Audit;
using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.Options;
using Microsoft.Extensions.Options;

namespace EnterpriseAiDocumentAssistant.Api.AiGateway;

public sealed class OpenAiGateway : IAiGateway
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IAuditLogger auditLogger;
    private readonly HttpClient httpClient;
    private readonly AiGatewayOptions options;

    public OpenAiGateway(
        HttpClient httpClient,
        IAuditLogger auditLogger,
        IOptions<AiGatewayOptions> options)
    {
        this.httpClient = httpClient;
        this.auditLogger = auditLogger;
        this.options = options.Value;
        this.httpClient.Timeout = TimeSpan.FromSeconds(Math.Max(1, this.options.TimeoutSeconds));
    }

    public async Task<ChatModelResponse> GenerateChatResponseAsync(
        ChatModelRequest request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            EnsureConfigured();
            using var httpRequest = BuildHttpRequest(request);
            using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"AI provider request failed with {(int)response.StatusCode}.");
            }

            var modelResponse = ParseResponse(responseJson, stopwatch.ElapsedMilliseconds);
            RecordAudit(
                true,
                modelResponse.LatencyMs,
                modelResponse.InputTokenEstimate,
                modelResponse.OutputTokenEstimate);

            return modelResponse;
        }
        catch
        {
            RecordAudit(false, stopwatch.ElapsedMilliseconds, null, null);
            throw;
        }
    }

    public IEnumerable<string> BuildResponseChunks(StructuredAssistantMessage message)
    {
        yield return message.Answer;
        yield return $" Confidence: {message.Confidence}.";

        if (message.SuggestedActions.Count > 0)
        {
            yield return $" Suggested action: {message.SuggestedActions[0]}";
        }
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            throw new InvalidOperationException(
                "AiGateway:ApiKey is required when AiGateway:Provider is OpenAI or AzureOpenAI.");
        }

        if (string.IsNullOrWhiteSpace(options.ChatModel))
        {
            throw new InvalidOperationException("AiGateway:ChatModel is required.");
        }
    }

    private HttpRequestMessage BuildHttpRequest(ChatModelRequest request)
    {
        var endpoint = options.Endpoint.TrimEnd('/');
        var provider = options.Provider.Trim();
        var isAzureOpenAi = string.Equals(provider, "AzureOpenAI", StringComparison.OrdinalIgnoreCase);
        var requestUri = isAzureOpenAi
            ? $"{endpoint}/openai/deployments/{Uri.EscapeDataString(options.ChatModel)}/chat/completions?api-version={Uri.EscapeDataString(options.ApiVersion)}"
            : $"{endpoint}/v1/chat/completions";

        var payload = BuildChatCompletionPayload(request, includeModel: !isAzureOpenAi);
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

    private object BuildChatCompletionPayload(ChatModelRequest request, bool includeModel)
    {
        var outputRules = request.Prompt.OutputRules.Count == 0
            ? string.Empty
            : $"{Environment.NewLine}Output rules:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", request.Prompt.OutputRules)}";

        var payload = new Dictionary<string, object?>
        {
            ["messages"] = new object[]
            {
                new
                {
                    role = "system",
                    content = $"{request.Prompt.SystemMessage}{Environment.NewLine}Return only JSON matching the required schema."
                },
                new
                {
                    role = "user",
                    content = $"{request.Prompt.UserMessage}{outputRules}"
                }
            },
            ["temperature"] = 0.2,
            ["response_format"] = new
            {
                type = "json_schema",
                json_schema = new
                {
                    name = "structured_assistant_message",
                    strict = true,
                    schema = new
                    {
                        type = "object",
                        additionalProperties = false,
                        required = new[] { "answer", "confidence", "citations", "suggestedActions" },
                        properties = new
                        {
                            answer = new
                            {
                                type = "string"
                            },
                            confidence = new
                            {
                                type = "string",
                                @enum = new[] { "low", "medium", "high" }
                            },
                            citations = new
                            {
                                type = "array",
                                items = new
                                {
                                    type = "string"
                                }
                            },
                            suggestedActions = new
                            {
                                type = "array",
                                items = new
                                {
                                    type = "string"
                                }
                            }
                        }
                    }
                }
            }
        };

        if (includeModel)
        {
            payload["model"] = options.ChatModel;
        }

        return payload;
    }

    private ChatModelResponse ParseResponse(string responseJson, long latencyMs)
    {
        using var document = JsonDocument.Parse(responseJson);
        var root = document.RootElement;
        var content = root
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("AI provider returned an empty message.");
        }

        var message = JsonSerializer.Deserialize<StructuredAssistantMessage>(content, JsonOptions)
            ?? throw new InvalidOperationException("AI provider returned invalid structured JSON.");

        var usage = root.TryGetProperty("usage", out var usageElement)
            ? usageElement
            : default;
        var inputTokens = ReadInt(usage, "prompt_tokens")
            ?? EstimateTokens(content);
        var outputTokens = ReadInt(usage, "completion_tokens")
            ?? EstimateTokens(message.Answer);

        return new ChatModelResponse(
            options.Provider,
            options.ChatModel,
            message,
            inputTokens,
            outputTokens,
            latencyMs);
    }

    private void RecordAudit(
        bool succeeded,
        long durationMs,
        int? inputTokenEstimate,
        int? outputTokenEstimate)
    {
        var metadata = new Dictionary<string, string>
        {
            ["model"] = options.ChatModel
        };

        if (inputTokenEstimate is not null)
        {
            metadata["inputTokenEstimate"] = inputTokenEstimate.Value.ToString();
        }

        if (outputTokenEstimate is not null)
        {
            metadata["outputTokenEstimate"] = outputTokenEstimate.Value.ToString();
        }

        auditLogger.Record(new AuditEventRequest(
            "ai_gateway",
            succeeded ? "chat_model_completed" : "chat_model_failed",
            options.Provider,
            succeeded,
            durationMs,
            metadata));
    }

    private static int? ReadInt(JsonElement usage, string propertyName)
    {
        if (usage.ValueKind != JsonValueKind.Object
            || !usage.TryGetProperty(propertyName, out var property)
            || property.ValueKind != JsonValueKind.Number)
        {
            return null;
        }

        return property.TryGetInt32(out var value) ? value : null;
    }

    private static int EstimateTokens(string value)
    {
        return Math.Max(1, (int)Math.Ceiling(value.Length / 4.0));
    }
}
