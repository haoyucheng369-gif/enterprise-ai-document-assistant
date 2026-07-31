using MongoDB.Bson.Serialization.Attributes;

namespace EnterpriseAiDocumentAssistant.Api.Conversations;

// One MongoDB document represents one complete request/response turn.
public sealed class MongoConversationTurnRecord
{
    [BsonId]
    public string Id { get; init; } = string.Empty;

    [BsonElement("workspaceId")]
    public string WorkspaceId { get; init; } = string.Empty;

    [BsonElement("documentId")]
    public string? DocumentId { get; init; }

    [BsonElement("createdAtUtc")]
    public DateTime CreatedAtUtc { get; init; }

    [BsonElement("userMessage")]
    public MongoConversationMessageRecord UserMessage { get; init; } = new();

    [BsonElement("assistantMessage")]
    public MongoConversationMessageRecord AssistantMessage { get; init; } = new();
}

// Persist the fields required to rebuild the conversation UI after an API restart.
public sealed class MongoConversationMessageRecord
{
    [BsonElement("id")]
    public string Id { get; init; } = string.Empty;

    [BsonElement("role")]
    public string Role { get; init; } = string.Empty;

    [BsonElement("content")]
    public string Content { get; init; } = string.Empty;

    [BsonElement("confidence")]
    public string? Confidence { get; init; }

    [BsonElement("citations")]
    public string[] Citations { get; init; } = [];

    [BsonElement("suggestedActions")]
    public string[] SuggestedActions { get; init; } = [];
}
