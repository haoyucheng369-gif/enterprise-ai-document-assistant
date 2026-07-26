using EnterpriseAiDocumentAssistant.Api.DocumentUpload;

namespace EnterpriseAiDocumentAssistant.Api.Documents;

public interface IDocumentRepository
{
    Task SaveAsync(DocumentUploadResponse document, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(string documentId, CancellationToken cancellationToken);

    DocumentUploadResponse? FindById(string documentId);

    IReadOnlyList<DocumentUploadResponse> ListRecent(int limit);
}
