using EnterpriseAiDocumentAssistant.Api.DocumentParsing;
using EnterpriseAiDocumentAssistant.Api.DocumentUpload;
using EnterpriseAiDocumentAssistant.Api.Options;
using EnterpriseAiDocumentAssistant.Api.Security;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EnterpriseAiDocumentAssistant.Api.Documents;

public sealed class MongoDocumentRepository : IDocumentRepository, IDocumentAccessPolicy
{
    private readonly IMongoCollection<MongoDocumentRecord> documents;
    private readonly ICurrentUserAccessor currentUserAccessor;

    public MongoDocumentRepository(
        IOptions<MongoDbOptions> options,
        ICurrentUserAccessor currentUserAccessor)
    {
        this.currentUserAccessor = currentUserAccessor;
        // The repository owns MongoDB connection details so upload/workspace code stays database-agnostic.
        var mongoOptions = options.Value;
        var client = new MongoClient(mongoOptions.ConnectionString);
        var database = client.GetDatabase(mongoOptions.DatabaseName);
        documents = database.GetCollection<MongoDocumentRecord>(mongoOptions.DocumentsCollectionName);

        EnsureIndexes();
    }

    public async Task SaveAsync(
        DocumentUploadResponse document,
        CancellationToken cancellationToken)
    {
        var record = ToRecord(document);

        // ReplaceOne with IsUpsert handles both first insert and future re-parse updates for the same document id.
        await documents.ReplaceOneAsync(
            item => item.Id == record.Id,
            record,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        string documentId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(documentId))
        {
            return false;
        }

        // Only an owner can delete; readers never reach the vector deletion step in the upload service.
        var result = await documents.DeleteOneAsync(
            Builders<MongoDocumentRecord>.Filter.And(
                Builders<MongoDocumentRecord>.Filter.Eq(item => item.Id, documentId),
                BuildOwnerFilter(currentUserAccessor.UserId)),
            cancellationToken);

        return result.DeletedCount > 0;
    }

    public DocumentUploadResponse? FindById(string documentId)
    {
        if (string.IsNullOrWhiteSpace(documentId))
        {
            return null;
        }

        // Every document consumer uses this read path, so ACL filtering happens before RAG, Skills, and Tools.
        var record = documents
            .Find(Builders<MongoDocumentRecord>.Filter.And(
                Builders<MongoDocumentRecord>.Filter.Eq(item => item.Id, documentId),
                BuildReadableFilter(currentUserAccessor.UserId)))
            .FirstOrDefault();

        return record is null ? null : ToResponse(record);
    }

    public IReadOnlyList<DocumentUploadResponse> ListRecent(int limit)
    {
        // Workspace receives only documents visible to the current user.
        return documents
            .Find(BuildReadableFilter(currentUserAccessor.UserId))
            .SortByDescending(item => item.UploadedAtUtc)
            .Limit(limit)
            .ToList()
            .Select(ToResponse)
            .ToArray();
    }

    public DocumentAccessLevel Evaluate(string documentId)
    {
        if (string.IsNullOrWhiteSpace(documentId))
        {
            return DocumentAccessLevel.NotFound;
        }

        // Read only ACL fields; callers need the decision, not unrestricted document content.
        var document = documents
            .Find(item => item.Id == documentId)
            .Project(item => new { item.OwnerId, item.AllowedUserIds })
            .FirstOrDefault();

        if (document is null)
        {
            return DocumentAccessLevel.NotFound;
        }

        var ownerId = NormalizeLegacyOwner(document.OwnerId);
        if (string.Equals(ownerId, currentUserAccessor.UserId, StringComparison.OrdinalIgnoreCase))
        {
            return DocumentAccessLevel.Owner;
        }

        return document.AllowedUserIds?.Contains(
            currentUserAccessor.UserId,
            StringComparer.OrdinalIgnoreCase) == true
            ? DocumentAccessLevel.Reader
            : DocumentAccessLevel.Denied;
    }

    private void EnsureIndexes()
    {
        // These indexes match the current UI reads: latest uploads first and status-based filtering later.
        documents.Indexes.CreateMany(
        [
            new CreateIndexModel<MongoDocumentRecord>(
                Builders<MongoDocumentRecord>.IndexKeys.Descending(item => item.UploadedAtUtc)),
            new CreateIndexModel<MongoDocumentRecord>(
                Builders<MongoDocumentRecord>.IndexKeys
                    .Ascending(item => item.Status)
                    .Descending(item => item.UploadedAtUtc)),
            new CreateIndexModel<MongoDocumentRecord>(
                Builders<MongoDocumentRecord>.IndexKeys
                    .Ascending(item => item.OwnerId)
                    .Ascending(item => item.AllowedUserIds)
                    .Descending(item => item.UploadedAtUtc))
        ]);
    }

    private static MongoDocumentRecord ToRecord(DocumentUploadResponse document)
    {
        // Convert the API read model into the MongoDB storage shape, including nested preview sections.
        return new MongoDocumentRecord
        {
            Id = document.Id,
            Title = document.Title,
            Type = document.Type,
            UpdatedAt = document.UpdatedAt,
            Status = document.Status,
            SizeBytes = document.SizeBytes,
            OwnerId = document.OwnerId,
            AllowedUserIds = document.AllowedUserIds,
            UploadedAtUtc = DateTime.UtcNow,
            Sections = document.Sections
                .Select(section => new MongoDocumentSectionRecord
                {
                    Label = section.Label,
                    Title = section.Title,
                    Body = section.Body
                })
                .ToArray()
        };
    }

    private static DocumentUploadResponse ToResponse(MongoDocumentRecord document)
    {
        // Convert MongoDB records back into the existing API contract used by controllers and frontend.
        return new DocumentUploadResponse(
            document.Id,
            document.Title,
            document.Type,
            document.UpdatedAt,
            document.Status,
            document.SizeBytes,
            document.Sections
                .Select(section => new DocumentPreviewSection(
                    section.Label,
                    section.Title,
                    section.Body))
                .ToArray(),
            NormalizeLegacyOwner(document.OwnerId),
            document.AllowedUserIds ?? []);
    }

    private static FilterDefinition<MongoDocumentRecord> BuildReadableFilter(string userId)
    {
        return Builders<MongoDocumentRecord>.Filter.Or(
            BuildOwnerFilter(userId),
            Builders<MongoDocumentRecord>.Filter.AnyEq(item => item.AllowedUserIds, userId));
    }

    private static FilterDefinition<MongoDocumentRecord> BuildOwnerFilter(string userId)
    {
        var ownerFilter = Builders<MongoDocumentRecord>.Filter.Eq(item => item.OwnerId, userId);
        if (!string.Equals(userId, HttpHeaderCurrentUserAccessor.DefaultUserId, StringComparison.OrdinalIgnoreCase))
        {
            return ownerFilter;
        }

        // Existing records created before ACL support belong to the default local user.
        return Builders<MongoDocumentRecord>.Filter.Or(
            ownerFilter,
            Builders<MongoDocumentRecord>.Filter.Eq(item => item.OwnerId, string.Empty),
            Builders<MongoDocumentRecord>.Filter.Exists("ownerId", false));
    }

    private static string NormalizeLegacyOwner(string? ownerId)
    {
        return string.IsNullOrWhiteSpace(ownerId)
            ? HttpHeaderCurrentUserAccessor.DefaultUserId
            : ownerId;
    }
}
