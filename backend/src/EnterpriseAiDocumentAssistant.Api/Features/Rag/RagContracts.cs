using EnterpriseAiDocumentAssistant.Api.Contracts;

namespace EnterpriseAiDocumentAssistant.Api.Rag;

public sealed record EmbeddingRequest(
    string Text,
    string? ProviderOverride = null);

public sealed record EmbeddingResponse(
    string Provider,
    string Model,
    float[] Vector);

public sealed record VectorChunk(
    string Provider,
    string DocumentId,
    string DocumentTitle,
    string ChunkId,
    string Label,
    string Title,
    string Text,
    float[] Embedding);

public sealed record VectorSearchResult(
    VectorChunk Chunk,
    double Score);

public enum RagRetrievalStatus
{
    Retrieved,
    NoDocumentSelected,
    DocumentNotFound,
    InsufficientEvidence
}

public sealed record RagRetrievalResult(
    RagRetrievalStatus Status,
    string PromptContext,
    IReadOnlyList<string> Citations)
{
    public bool HasEvidence => Status == RagRetrievalStatus.Retrieved;
}
