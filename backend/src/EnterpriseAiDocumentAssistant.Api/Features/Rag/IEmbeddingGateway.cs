namespace EnterpriseAiDocumentAssistant.Api.Rag;

public interface IEmbeddingGateway
{
    Task<EmbeddingResponse> GenerateEmbeddingAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken);
}
