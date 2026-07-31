using MongoDB.Bson.Serialization.Attributes;

namespace EnterpriseAiDocumentAssistant.Api.Documents;

// Storage model for MongoDB. It is separate from API contracts so database fields can evolve independently.
public sealed class MongoDocumentRecord
{
    [BsonId]
    public string Id { get; init; } = string.Empty;

    // BsonElement keeps MongoDB field names in lower camel case while C# properties stay PascalCase.
    [BsonElement("title")]
    public string Title { get; init; } = string.Empty;

    [BsonElement("type")]
    public string Type { get; init; } = string.Empty;

    [BsonElement("updatedAt")]
    public string UpdatedAt { get; init; } = string.Empty;

    [BsonElement("status")]
    public string Status { get; init; } = string.Empty;

    [BsonElement("sizeBytes")]
    public long SizeBytes { get; init; }

    [BsonElement("ownerId")]
    public string OwnerId { get; init; } = string.Empty;

    [BsonElement("allowedUserIds")]
    public IReadOnlyList<string> AllowedUserIds { get; init; } = [];

    [BsonElement("uploadedAtUtc")]
    public DateTime UploadedAtUtc { get; init; }

    [BsonElement("sections")]
    public IReadOnlyList<MongoDocumentSectionRecord> Sections { get; init; } = [];
}

// Embedded child document; sections are stored inside the parent document instead of a separate collection.
public sealed class MongoDocumentSectionRecord
{
    [BsonElement("label")]
    public string Label { get; init; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; init; } = string.Empty;

    [BsonElement("body")]
    public string Body { get; init; } = string.Empty;
}
