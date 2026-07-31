using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EnterpriseAiDocumentAssistant.Api.Audit;
using EnterpriseAiDocumentAssistant.Api.Options;
using EnterpriseAiDocumentAssistant.Api.Security;
using Microsoft.Extensions.Options;

namespace EnterpriseAiDocumentAssistant.Api.Rag;

public sealed class RoutingEmbeddingGateway : IEmbeddingGateway
{
    private const int MockVectorSize = 64;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IAuditLogger auditLogger;
    private readonly ICurrentUserAccessor currentUserAccessor;
    private readonly HttpClient httpClient;
    private readonly AiGatewayOptions options;

    public RoutingEmbeddingGateway(
        HttpClient httpClient,
        IAuditLogger auditLogger,
        ICurrentUserAccessor currentUserAccessor,
        IOptions<AiGatewayOptions> options)
    {
        this.httpClient = httpClient;
        this.auditLogger = auditLogger;
        this.currentUserAccessor = currentUserAccessor;
        this.options = options.Value;
        this.httpClient.Timeout = TimeSpan.FromSeconds(Math.Max(1, this.options.TimeoutSeconds));
    }

    public async Task<EmbeddingResponse> GenerateEmbeddingAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken)
    {
        // Route embedding generation through Mock, OpenAI, or Azure OpenAI.
        var provider = ResolveProvider(request.ProviderOverride);
        var stopwatch = Stopwatch.StartNew();

        if (IsMock(provider))
        {
            // Deterministic Mock vectors keep local RAG debuggable without API cost.
            var embedding = new EmbeddingResponse(
                "Mock",
                "deterministic-local-embedding",
                BuildMockEmbedding(request.Text));
            RecordAudit(
                embedding.Provider,
                embedding.Model,
                true,
                stopwatch.ElapsedMilliseconds,
                EstimateTokens(request.Text));
            return embedding;
        }

        try
        {
            EnsureConfigured(provider);
            using var httpRequest = BuildHttpRequest(request.Text, provider);
            using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Embedding provider request failed with {(int)response.StatusCode}.");
            }

            var embedding = ParseEmbeddingResponse(responseJson);
            RecordAudit(
                provider,
                options.EmbeddingModel,
                true,
                stopwatch.ElapsedMilliseconds,
                EstimateTokens(request.Text));

            return new EmbeddingResponse(provider, options.EmbeddingModel, embedding);
        }
        catch
        {
            RecordAudit(
                provider,
                options.EmbeddingModel,
                false,
                stopwatch.ElapsedMilliseconds,
                EstimateTokens(request.Text));
            throw;
        }
    }

    private HttpRequestMessage BuildHttpRequest(string text, string provider)
    {
        // OpenAI and Azure OpenAI use different URLs and authentication for embedding calls.
        var endpoint = options.Endpoint.TrimEnd('/');
        var isAzureOpenAi = string.Equals(provider, "AzureOpenAI", StringComparison.OrdinalIgnoreCase);
        var requestUri = isAzureOpenAi
            ? $"{endpoint}/openai/deployments/{Uri.EscapeDataString(options.EmbeddingModel)}/embeddings?api-version={Uri.EscapeDataString(options.ApiVersion)}"
            : $"{endpoint}/v1/embeddings";

        object payload = isAzureOpenAi
            ? new { input = text }
            : new { model = options.EmbeddingModel, input = text };

        var content = new StringContent(
            JsonSerializer.Serialize(payload, JsonOptions),
            Encoding.UTF8);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = content
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

    private void EnsureConfigured(string provider)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            throw new InvalidOperationException(
                $"AiGateway:ApiKey is required when the selected embedding provider is {provider}.");
        }

        if (string.IsNullOrWhiteSpace(options.EmbeddingModel))
        {
            throw new InvalidOperationException("AiGateway:EmbeddingModel is required.");
        }
    }

    private void RecordAudit(
        string provider,
        string model,
        bool succeeded,
        long durationMs,
        int inputTokenEstimate)
    {
        auditLogger.Record(new AuditEventRequest(
            "ai_gateway",
            succeeded ? "embedding_completed" : "embedding_failed",
            provider,
            succeeded,
            durationMs,
            new Dictionary<string, string>
            {
                ["model"] = model,
                ["userId"] = currentUserAccessor.UserId,
                ["inputTokenEstimate"] = inputTokenEstimate.ToString()
            }));
    }

    private string ResolveProvider(string? requestedProvider)
    {
        return string.IsNullOrWhiteSpace(requestedProvider)
            ? options.Provider
            : requestedProvider.Trim();
    }

    private static bool IsMock(string provider)
    {
        return string.Equals(provider, "Mock", StringComparison.OrdinalIgnoreCase);
    }

    private static int EstimateTokens(string value)
    {
        return Math.Max(1, (int)Math.Ceiling(value.Length / 4.0));
    }

    private static float[] ParseEmbeddingResponse(string responseJson)
    {
        // Provider JSON data[0].embedding becomes the vector used by RAG.
        using var document = JsonDocument.Parse(responseJson);
        var embeddingElement = document.RootElement
            .GetProperty("data")[0]
            .GetProperty("embedding");

        return embeddingElement
            .EnumerateArray()
            .Select(value => value.GetSingle())
            .ToArray();
    }

    private static float[] BuildMockEmbedding(string text)
    {
        // Hash tokens into a small fixed-size vector for deterministic local retrieval.
        var vector = new float[MockVectorSize];
        var tokens = text
            .ToLowerInvariant()
            .Split([' ', '\r', '\n', '\t', '.', ',', ';', ':', '/', '\\', '-', '_', '(', ')'], StringSplitOptions.RemoveEmptyEntries);

        foreach (var token in tokens)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            var index = BitConverter.ToUInt16(hash, 0) % MockVectorSize;
            vector[index] += 1;
        }

        // Normalize so cosine similarity behaves like a real vector comparison.
        Normalize(vector);
        return vector;
    }

    private static void Normalize(float[] vector)
    {
        var magnitude = Math.Sqrt(vector.Sum(value => value * value));
        if (magnitude == 0)
        {
            return;
        }

        for (var index = 0; index < vector.Length; index++)
        {
            vector[index] = (float)(vector[index] / magnitude);
        }
    }
}
