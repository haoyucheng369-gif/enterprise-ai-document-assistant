namespace EnterpriseAiDocumentAssistant.Api.Options;

// Runtime configuration for the MongoDB document persistence boundary.
public sealed class MongoDbOptions
{
    public const string SectionName = "MongoDb";

    public string ConnectionString { get; init; } = "mongodb://localhost:27017";

    public string DatabaseName { get; init; } = "enterprise_ai_document_assistant";

    public string DocumentsCollectionName { get; init; } = "documents";

    public string ConversationsCollectionName { get; init; } = "conversation_turns";
}
