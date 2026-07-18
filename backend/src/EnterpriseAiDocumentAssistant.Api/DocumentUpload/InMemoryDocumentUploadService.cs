using System.Collections.Concurrent;
using EnterpriseAiDocumentAssistant.Api.Audit;
using EnterpriseAiDocumentAssistant.Api.Services;

namespace EnterpriseAiDocumentAssistant.Api.DocumentUpload;

public sealed class InMemoryDocumentUploadService : IDocumentUploadService
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt",
        ".md",
        ".pdf",
        ".docx"
    };

    private readonly ConcurrentQueue<DocumentUploadResponse> uploads = new();
    private readonly IAuditLogger auditLogger;
    private readonly ISystemClock systemClock;

    public InMemoryDocumentUploadService(
        IAuditLogger auditLogger,
        ISystemClock systemClock)
    {
        this.auditLogger = auditLogger;
        this.systemClock = systemClock;
    }

    public Task<DocumentUploadResult> UploadAsync(
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return Task.FromResult(Failed("File is required."));
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return Task.FromResult(Failed("File size must be 5 MB or smaller."));
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            return Task.FromResult(Failed("Supported file types are .txt, .md, .pdf, and .docx."));
        }

        cancellationToken.ThrowIfCancellationRequested();

        // The first ingestion step stores metadata only; parsing and content storage come later.
        var document = new DocumentUploadResponse(
            $"upload-{Guid.NewGuid():N}",
            Path.GetFileNameWithoutExtension(file.FileName),
            extension.TrimStart('.').ToUpperInvariant(),
            systemClock.UtcNow.ToString("yyyy-MM-dd"),
            "Uploaded",
            file.Length);

        uploads.Enqueue(document);
        auditLogger.Record(new AuditEventRequest(
            "document",
            "document_uploaded",
            "api/documents/upload",
            true,
            0,
            new Dictionary<string, string>
            {
                ["documentId"] = document.Id,
                ["fileName"] = file.FileName,
                ["sizeBytes"] = file.Length.ToString()
            }));

        return Task.FromResult(new DocumentUploadResult(true, document, null));
    }

    public IReadOnlyList<DocumentUploadResponse> ListRecent()
    {
        return uploads
            .Reverse()
            .Take(20)
            .ToArray();
    }

    private static DocumentUploadResult Failed(string error)
    {
        return new DocumentUploadResult(false, null, error);
    }
}
