using System.Collections.Concurrent;
using EnterpriseAiDocumentAssistant.Api.Audit;
using EnterpriseAiDocumentAssistant.Api.DocumentParsing;
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
    private readonly IDocumentChunker documentChunker;
    private readonly IDocumentTextExtractor documentTextExtractor;
    private readonly ISystemClock systemClock;

    public InMemoryDocumentUploadService(
        IAuditLogger auditLogger,
        IDocumentChunker documentChunker,
        IDocumentTextExtractor documentTextExtractor,
        ISystemClock systemClock)
    {
        this.auditLogger = auditLogger;
        this.documentChunker = documentChunker;
        this.documentTextExtractor = documentTextExtractor;
        this.systemClock = systemClock;
    }

    public async Task<DocumentUploadResult> UploadAsync(
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return Failed("File is required.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return Failed("File size must be 5 MB or smaller.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            return Failed("Supported file types are .txt, .md, .pdf, and .docx.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        var extraction = await documentTextExtractor.ExtractAsync(file, extension, cancellationToken);
        var sections = documentChunker.BuildPreviewSections(extraction.Text, extraction.Warnings);

        // Upload now returns lightweight parsed preview sections; full indexing remains a later RAG step.
        var document = new DocumentUploadResponse(
            $"upload-{Guid.NewGuid():N}",
            Path.GetFileNameWithoutExtension(file.FileName),
            extension.TrimStart('.').ToUpperInvariant(),
            systemClock.UtcNow.ToString("yyyy-MM-dd"),
            "Parsed",
            file.Length,
            sections);

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

        return new DocumentUploadResult(true, document, null);
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
