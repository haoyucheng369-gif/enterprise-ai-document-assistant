using EnterpriseAiDocumentAssistant.Api.Contracts;

namespace EnterpriseAiDocumentAssistant.Api.ConversationMemory;

public interface IConversationMemoryBuilder
{
    ConversationMemoryContext Build(IReadOnlyList<MessageResponse>? history);
}
