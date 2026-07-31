using EnterpriseAiDocumentAssistant.Api.Audit;
using EnterpriseAiDocumentAssistant.Api.DocumentParsing;
using EnterpriseAiDocumentAssistant.Api.Documents;
using EnterpriseAiDocumentAssistant.Api.Rag;
using EnterpriseAiDocumentAssistant.Api.Security;
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
    private readonly IRagService ragService;
    private readonly ISystemClock systemClock;
    private readonly ICurrentUserAccessor currentUserAccessor;

    public DocumentUploadService(
        IAuditLogger auditLogger,
        IDocumentChunker documentChunker,
        IDocumentRepository documentRepository,
        IDocumentTextExtractor documentTextExtractor,
        IRagService ragService,
        ISystemClock systemClock,
        ICurrentUserAccessor currentUserAccessor)
    {
        this.auditLogger = auditLogger;
        this.documentChunker = documentChunker;
        this.documentRepository = documentRepository;
        this.documentTextExtractor = documentTextExtractor;
        this.ragService = ragService;
        this.systemClock = systemClock;
        this.currentUserAccessor = currentUserAccessor;
    }

    public async Task<DocumentUploadResult> UploadAsync(
        IFormFile? file,
        string? aiProvider,
        IReadOnlyList<string>? allowedUserIds,
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
            sections,
            currentUserAccessor.UserId,
            NormalizeAllowedUsers(allowedUserIds, currentUserAccessor.UserId));

        // Step 3: persist the parsed document so workspace and skills can read it after API restart.
        await documentRepository.SaveAsync(document, cancellationToken);

        // Step 4: build a best-effort vector index. MongoDB remains the source of truth if indexing fails.
        await TryIndexDocumentForRagAsync(document, aiProvider, cancellationToken);

        // Step 5: record an audit event for observability; this is separate from document persistence.
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
                ["sizeBytes"] = file.Length.ToString(),
                ["ownerId"] = document.OwnerId
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
        // Delete removes parsed metadata/chunks from MongoDB and clears provider-specific vector entries.
        var deleted = await documentRepository.DeleteAsync(documentId, cancellationToken);
        if (deleted)
        {
            await ragService.DeleteDocumentAsync(documentId, cancellationToken);
        }

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

    private static IReadOnlyList<string> NormalizeAllowedUsers(
        IReadOnlyList<string>? allowedUserIds,
        string ownerId)
    {
        return allowedUserIds?
            .Select(userId => userId.Trim().ToLowerInvariant())
            .Where(userId => userId.Length > 0)
            .Where(userId => !string.Equals(userId, ownerId, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];
    }

    private async Task TryIndexDocumentForRagAsync(
        DocumentUploadResponse document,
        string? aiProvider,
        CancellationToken cancellationToken)
    {
        try
        {
            // Upload Index Step: vector index is derived state, so failure should not reject the saved document.
            await ragService.IndexDocumentAsync(document, aiProvider, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            // If this fails, RAG retrieval can rebuild vectors lazily from MongoDB document sections later.
            auditLogger.Record(new AuditEventRequest(
                "rag",
                "document_index_failed",
                "api/documents/upload",
                false,
                0,
                new Dictionary<string, string>
                {
                    ["documentId"] = document.Id,
                    ["aiProvider"] = aiProvider ?? string.Empty,
                    ["errorType"] = exception.GetType().Name
                }));
        }
    }
}
