using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EnterpriseAiDocumentAssistant.Api.Conversations;

public sealed class MongoConversationRepository : IConversationRepository
{
    private const string DefaultWorkspaceId = "default";
    private readonly IMongoCollection<MongoConversationTurnRecord> conversationTurns;

    public MongoConversationRepository(IOptions<MongoDbOptions> options)
    {
        var mongoOptions = options.Value;
        var client = new MongoClient(mongoOptions.ConnectionString);
        var database = client.GetDatabase(mongoOptions.DatabaseName);
        conversationTurns = database.GetCollection<MongoConversationTurnRecord>(
            mongoOptions.ConversationsCollectionName);

        EnsureIndexes();
    }

    public async Task AppendTurnAsync(
        string? documentId,
        MessageResponse userMessage,
        MessageResponse assistantMessage,
        CancellationToken cancellationToken)
    {
        // A single insert keeps the user request and its validated assistant response together.
        var turn = new MongoConversationTurnRecord
        {
            Id = $"turn-{Guid.NewGuid():N}",
            WorkspaceId = DefaultWorkspaceId,
            DocumentId = documentId,
            CreatedAtUtc = DateTime.UtcNow,
            UserMessage = ToRecord(userMessage),
            AssistantMessage = ToRecord(assistantMessage)
        };

        await conversationTurns.InsertOneAsync(turn, cancellationToken: cancellationToken);
    }

    public IReadOnlyList<MessageResponse> ListRecent(int turnLimit)
    {
        if (turnLimit <= 0)
        {
            return [];
        }

        // Query newest turns efficiently, then restore chronological order for the chat UI.
        var turns = conversationTurns
            .Find(turn => turn.WorkspaceId == DefaultWorkspaceId)
            .SortByDescending(turn => turn.CreatedAtUtc)
            .Limit(turnLimit)
            .ToList();

        turns.Reverse();

        return turns
            .SelectMany(turn => new[]
            {
                ToResponse(turn.UserMessage),
                ToResponse(turn.AssistantMessage)
            })
            .ToArray();
    }

    private void EnsureIndexes()
    {
        // Workspace history reads use this compound index to fetch the latest turns.
        conversationTurns.Indexes.CreateOne(
            new CreateIndexModel<MongoConversationTurnRecord>(
                Builders<MongoConversationTurnRecord>.IndexKeys
                    .Ascending(turn => turn.WorkspaceId)
                    .Descending(turn => turn.CreatedAtUtc)));
    }

    private static MongoConversationMessageRecord ToRecord(MessageResponse message)
    {
        return new MongoConversationMessageRecord
        {
            Id = message.Id,
            Role = message.Role,
            Content = message.Content,
            Confidence = message.Confidence,
            Citations = message.Citations?.ToArray() ?? [],
            SuggestedActions = message.SuggestedActions?.ToArray() ?? []
        };
    }

    private static MessageResponse ToResponse(MongoConversationMessageRecord message)
    {
        return new MessageResponse(
            message.Id,
            message.Role,
            message.Content,
            message.Confidence,
            message.Citations,
            message.SuggestedActions);
    }
}
