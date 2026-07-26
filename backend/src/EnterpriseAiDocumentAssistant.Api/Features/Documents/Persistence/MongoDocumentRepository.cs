using EnterpriseAiDocumentAssistant.Api.DocumentParsing;
using EnterpriseAiDocumentAssistant.Api.DocumentUpload;
using EnterpriseAiDocumentAssistant.Api.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EnterpriseAiDocumentAssistant.Api.Documents;

public sealed class MongoDocumentRepository : IDocumentRepository
{
    private readonly IMongoCollection<MongoDocumentRecord> documents;

    public MongoDocumentRepository(IOptions<MongoDbOptions> options)
    {
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

        // Delete the persisted document read model; future vector/chunk collections should delete by the same document id.
        var result = await documents.DeleteOneAsync(
            item => item.Id == documentId,
            cancellationToken);

        return result.DeletedCount > 0;
    }

    public DocumentUploadResponse? FindById(string documentId)
    {
        if (string.IsNullOrWhiteSpace(documentId))
        {
            return null;
        }

        // Skills and tools call this path when they need the selected uploaded document by id.
        var record = documents
            .Find(item => item.Id == documentId)
            .FirstOrDefault();

        return record is null ? null : ToResponse(record);
    }

    public IReadOnlyList<DocumentUploadResponse> ListRecent(int limit)
    {
        // Workspace uses this query to show the latest persisted uploads.
        return documents
            .Find(FilterDefinition<MongoDocumentRecord>.Empty)
            .SortByDescending(item => item.UploadedAtUtc)
            .Limit(limit)
            .ToList()
            .Select(ToResponse)
            .ToArray();
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
                .ToArray());
    }
}
