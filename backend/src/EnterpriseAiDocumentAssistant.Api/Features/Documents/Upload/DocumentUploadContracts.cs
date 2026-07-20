using EnterpriseAiDocumentAssistant.Api.DocumentParsing;

namespace EnterpriseAiDocumentAssistant.Api.DocumentUpload;

public sealed record DocumentUploadResponse(
    string Id,
    string Title,
    string Type,
    string UpdatedAt,
    string Status,
    long SizeBytes,
    IReadOnlyList<DocumentPreviewSection> Sections);

public sealed class DocumentUploadForm
{
    public IFormFile? File { get; init; }
}

public sealed record DocumentUploadResult(
    bool Succeeded,
    DocumentUploadResponse? Document,
    string? Error);
