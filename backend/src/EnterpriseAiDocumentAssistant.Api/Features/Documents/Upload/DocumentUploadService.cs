using EnterpriseAiDocumentAssistant.Api.Audit;
using EnterpriseAiDocumentAssistant.Api.DocumentParsing;
using EnterpriseAiDocumentAssistant.Api.Documents;
using EnterpriseAiDocumentAssistant.Api.Services;

namespace EnterpriseAiDocumentAssistant.Api.DocumentUpload;

public sealed class DocumentUploadService : IDocumentUploadService
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt",
        ".md",
        ".pdf",
        ".docx"
    };

    private readonly IAuditLogger auditLogger;
    private readonly IDocumentChunker documentChunker;
    private readonly IDocumentRepository documentRepository;
    private readonly IDocumentTextExtractor documentTextExtractor;
    private readonly ISystemClock systemClock;

    public DocumentUploadService(
        IAuditLogger auditLogger,
        IDocumentChunker documentChunker,
        IDocumentRepository documentRepository,
        IDocumentTextExtractor documentTextExtractor,
        ISystemClock systemClock)
    {
        this.auditLogger = auditLogger;
        this.documentChunker = documentChunker;
        this.documentRepository = documentRepository;
        this.documentTextExtractor = documentTextExtractor;
        this.systemClock = systemClock;
    }

    public async Task<DocumentUploadResult> UploadAsync(
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        // Step 1: validate the HTTP file before parsing or storing anything.
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

        // Step 2: extract plain text from the supported file type, then split it into preview sections.
        var extraction = await documentTextExtractor.ExtractAsync(file, extension, cancellationToken);
        var sections = documentChunker.BuildPreviewSections(extraction.Text, extraction.Warnings);

        // Upload creates the document read model that MongoDB will persist for workspace and skill access.
        var document = new DocumentUploadResponse(
            $"upload-{Guid.NewGuid():N}",
            Path.GetFileNameWithoutExtension(file.FileName),
            extension.TrimStart('.').ToUpperInvariant(),
            systemClock.UtcNow.ToString("yyyy-MM-dd"),
            "Parsed",
            file.Length,
            sections);

        // Step 3: persist the parsed document so workspace and skills can read it after API restart.
        await documentRepository.SaveAsync(document, cancellationToken);

        // Step 4: record an audit event for observability; this is separate from document persistence.
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
        // DocumentsController exposes this for quick backend checks; workspace uses the repository directly.
        return documentRepository.ListRecent(20);
    }

    public async Task<bool> DeleteAsync(string documentId, CancellationToken cancellationToken)
    {
        // Delete currently removes parsed metadata/chunks from MongoDB; raw file and vector cleanup can attach here later.
        var deleted = await documentRepository.DeleteAsync(documentId, cancellationToken);

        auditLogger.Record(new AuditEventRequest(
            "document",
            "document_deleted",
            $"api/documents/{documentId}",
            deleted,
            0,
            new Dictionary<string, string>
            {
                ["documentId"] = documentId
            }));

        return deleted;
    }

    private static DocumentUploadResult Failed(string error)
    {
        return new DocumentUploadResult(false, null, error);
    }
}
