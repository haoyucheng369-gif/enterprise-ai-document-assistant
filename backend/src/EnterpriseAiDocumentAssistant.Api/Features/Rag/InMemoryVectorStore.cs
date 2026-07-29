using System.Collections.Concurrent;

namespace EnterpriseAiDocumentAssistant.Api.Rag;

public sealed class InMemoryVectorStore : IVectorStore
{
    private readonly ConcurrentDictionary<string, IReadOnlyList<VectorChunk>> documents = new();

    public Task<bool> HasDocumentAsync(
        string provider,
        string documentId,
        CancellationToken cancellationToken)
    {
        // Provider is part of the key because vectors from different embedding models are not comparable.
        return Task.FromResult(documents.ContainsKey(BuildKey(provider, documentId)));
    }

    public Task UpsertDocumentChunksAsync(
        string provider,
        string documentId,
        IReadOnlyList<VectorChunk> chunks,
        CancellationToken cancellationToken)
    {
        // Store all chunk vectors for one document/provider pair as replaceable derived state.
        documents[BuildKey(provider, documentId)] = chunks;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        string provider,
        string documentId,
        float[] queryVector,
        int topK,
        CancellationToken cancellationToken)
    {
        if (!documents.TryGetValue(BuildKey(provider, documentId), out var chunks) || chunks.Count == 0)
        {
            return Task.FromResult<IReadOnlyList<VectorSearchResult>>([]);
        }

        // Score every chunk with cosine similarity and return the best top-k matches.
        var results = chunks
            .Select(chunk => new VectorSearchResult(chunk, CosineSimilarity(queryVector, chunk.Embedding)))
            .OrderByDescending(result => result.Score)
            .Take(Math.Max(1, topK))
            .ToArray();

        return Task.FromResult<IReadOnlyList<VectorSearchResult>>(results);
    }

    public Task DeleteDocumentAsync(
        string documentId,
        CancellationToken cancellationToken)
    {
        // Delete all provider-specific vector entries for this document.
        foreach (var key in documents.Keys.Where(key => key.EndsWith($":{documentId}", StringComparison.Ordinal)))
        {
            documents.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }

    private static string BuildKey(string provider, string documentId)
    {
        return $"{provider.Trim().ToUpperInvariant()}:{documentId}";
    }

    private static double CosineSimilarity(float[] left, float[] right)
    {
        // Cosine similarity measures direction closeness between question and chunk vectors.
        var length = Math.Min(left.Length, right.Length);
        if (length == 0)
        {
            return 0;
        }

        double dot = 0;
        double leftMagnitude = 0;
        double rightMagnitude = 0;

        for (var index = 0; index < length; index++)
        {
            dot += left[index] * right[index];
            leftMagnitude += left[index] * left[index];
            rightMagnitude += right[index] * right[index];
        }

        if (leftMagnitude == 0 || rightMagnitude == 0)
        {
            return 0;
        }

        return dot / (Math.Sqrt(leftMagnitude) * Math.Sqrt(rightMagnitude));
    }
}
