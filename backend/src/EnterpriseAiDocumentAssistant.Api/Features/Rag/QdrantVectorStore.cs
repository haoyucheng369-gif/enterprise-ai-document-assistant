using System.Security.Cryptography;
using System.Text;
using EnterpriseAiDocumentAssistant.Api.Options;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using static Qdrant.Client.Grpc.Conditions;

namespace EnterpriseAiDocumentAssistant.Api.Rag;

public sealed class QdrantVectorStore : IVectorStore
{
    private static readonly string[] SupportedProviders = ["Mock", "OpenAI", "AzureOpenAI"];

    private readonly QdrantClient client;
    private readonly QdrantOptions options;
    private readonly SemaphoreSlim collectionLock = new(1, 1);

    public QdrantVectorStore(IOptions<QdrantOptions> options)
    {
        this.options = options.Value;
        client = new QdrantClient(this.options.Host, this.options.GrpcPort);
    }

    public async Task<bool> HasDocumentAsync(
        string provider,
        string documentId,
        CancellationToken cancellationToken)
    {
        var collectionName = BuildCollectionName(provider);
        if (!await client.CollectionExistsAsync(collectionName, cancellationToken))
        {
            return false;
        }

        var count = await client.CountAsync(
            collectionName,
            filter: BuildDocumentFilter(documentId),
            exact: false,
            cancellationToken: cancellationToken);

        return count > 0;
    }

    public async Task UpsertDocumentChunksAsync(
        string provider,
        string documentId,
        IReadOnlyList<VectorChunk> chunks,
        CancellationToken cancellationToken)
    {
        if (chunks.Count == 0)
        {
            return;
        }

        var collectionName = BuildCollectionName(provider);
        await EnsureCollectionAsync(
            collectionName,
            (ulong)chunks[0].Embedding.Length,
            cancellationToken);

        // Replace derived vectors for the document so removed or re-chunked sections do not remain.
        await client.DeleteAsync(
            collectionName,
            BuildDocumentFilter(documentId),
            wait: true,
            cancellationToken: cancellationToken);

        var points = chunks.Select(ToPoint).ToArray();
        await client.UpsertAsync(
            collectionName,
            points,
            wait: true,
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        string provider,
        string documentId,
        float[] queryVector,
        int topK,
        CancellationToken cancellationToken)
    {
        var collectionName = BuildCollectionName(provider);
        if (!await client.CollectionExistsAsync(collectionName, cancellationToken))
        {
            return [];
        }

        // Qdrant performs cosine nearest-neighbor search and returns source text from payload.
        var points = await client.SearchAsync(
            collectionName,
            queryVector,
            filter: BuildDocumentFilter(documentId),
            limit: (ulong)Math.Max(1, topK),
            payloadSelector: true,
            vectorsSelector: false,
            cancellationToken: cancellationToken);

        return points
            .Select(point => new VectorSearchResult(
                new VectorChunk(
                    provider,
                    ReadString(point.Payload, "documentId"),
                    ReadString(point.Payload, "documentTitle"),
                    ReadString(point.Payload, "chunkId"),
                    ReadString(point.Payload, "label"),
                    ReadString(point.Payload, "title"),
                    ReadString(point.Payload, "text"),
                    []),
                point.Score))
            .ToArray();
    }

    public async Task DeleteDocumentAsync(
        string documentId,
        CancellationToken cancellationToken)
    {
        // A document may have vectors from multiple embedding providers, so remove each provider copy.
        foreach (var provider in SupportedProviders)
        {
            var collectionName = BuildCollectionName(provider);
            if (!await client.CollectionExistsAsync(collectionName, cancellationToken))
            {
                continue;
            }

            await client.DeleteAsync(
                collectionName,
                BuildDocumentFilter(documentId),
                wait: true,
                cancellationToken: cancellationToken);
        }
    }

    private async Task EnsureCollectionAsync(
        string collectionName,
        ulong vectorSize,
        CancellationToken cancellationToken)
    {
        if (await client.CollectionExistsAsync(collectionName, cancellationToken))
        {
            return;
        }

        await collectionLock.WaitAsync(cancellationToken);
        try
        {
            if (await client.CollectionExistsAsync(collectionName, cancellationToken))
            {
                return;
            }

            // Each provider uses a separate collection because embedding dimensions may differ.
            await client.CreateCollectionAsync(
                collectionName,
                new VectorParams
                {
                    Size = vectorSize,
                    Distance = Distance.Cosine
                },
                cancellationToken: cancellationToken);
        }
        finally
        {
            collectionLock.Release();
        }
    }

    private PointStruct ToPoint(VectorChunk chunk)
    {
        return new PointStruct
        {
            Id = CreateDeterministicPointId(chunk.Provider, chunk.ChunkId),
            Vectors = chunk.Embedding,
            Payload =
            {
                ["provider"] = chunk.Provider,
                ["documentId"] = chunk.DocumentId,
                ["documentTitle"] = chunk.DocumentTitle,
                ["chunkId"] = chunk.ChunkId,
                ["label"] = chunk.Label,
                ["title"] = chunk.Title,
                ["text"] = chunk.Text
            }
        };
    }

    private Filter BuildDocumentFilter(string documentId)
    {
        return new Filter
        {
            Must = { MatchKeyword("documentId", documentId) }
        };
    }

    private string BuildCollectionName(string provider)
    {
        var normalizedProvider = new string(provider
            .Trim()
            .ToLowerInvariant()
            .Where(character => char.IsLetterOrDigit(character) || character == '_')
            .ToArray());

        return $"{options.CollectionPrefix}_{normalizedProvider}";
    }

    private static Guid CreateDeterministicPointId(string provider, string chunkId)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{provider}:{chunkId}"));
        return new Guid(bytes.AsSpan(0, 16));
    }

    private static string ReadString(
        IReadOnlyDictionary<string, Value> payload,
        string key)
    {
        return payload.TryGetValue(key, out var value)
            ? value.StringValue
            : string.Empty;
    }
}
