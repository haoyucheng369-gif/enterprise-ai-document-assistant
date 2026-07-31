namespace EnterpriseAiDocumentAssistant.Api.DocumentUpload;

public interface IDocumentUploadService
{
    Task<DocumentUploadResult> UploadAsync(
        IFormFile? file,
        string? aiProvider,
        IReadOnlyList<string>? allowedUserIds,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(string documentId, CancellationToken cancellationToken);

    IReadOnlyList<DocumentUploadResponse> ListRecent();
}
