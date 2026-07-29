using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.DocumentUpload;

namespace EnterpriseAiDocumentAssistant.Api.Rag;

public interface IRagService
{
    Task IndexDocumentAsync(
        DocumentUploadResponse document,
        string? providerOverride,
        CancellationToken cancellationToken);

    Task DeleteDocumentAsync(
        string documentId,
        CancellationToken cancellationToken);

    Task<RagRetrievalResult> RetrieveAsync(
        ChatRequest request,
        CancellationToken cancellationToken);
}
