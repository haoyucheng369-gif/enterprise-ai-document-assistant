namespace EnterpriseAiDocumentAssistant.Api.DocumentUpload;

public interface IDocumentUploadService
{
    Task<DocumentUploadResult> UploadAsync(
        IFormFile? file,
        CancellationToken cancellationToken);

    IReadOnlyList<DocumentUploadResponse> ListRecent();
}
