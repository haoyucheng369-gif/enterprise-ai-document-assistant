namespace EnterpriseAiDocumentAssistant.Api.Rag;

public interface IVectorStore
{
    Task<bool> HasDocumentAsync(
        string provider,
        string documentId,
        CancellationToken cancellationToken);

    Task UpsertDocumentChunksAsync(
        string provider,
        string documentId,
        IReadOnlyList<VectorChunk> chunks,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        string provider,
        string documentId,
        float[] queryVector,
        int topK,
        CancellationToken cancellationToken);

    Task DeleteDocumentAsync(
        string documentId,
        CancellationToken cancellationToken);
}
