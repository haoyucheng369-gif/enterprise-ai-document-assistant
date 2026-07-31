using EnterpriseAiDocumentAssistant.Api.Contracts;

namespace EnterpriseAiDocumentAssistant.Api.Conversations;

public interface IConversationRepository
{
    Task AppendTurnAsync(
        string? documentId,
        MessageResponse userMessage,
        MessageResponse assistantMessage,
        CancellationToken cancellationToken);

    IReadOnlyList<MessageResponse> ListRecent(int turnLimit);
}
