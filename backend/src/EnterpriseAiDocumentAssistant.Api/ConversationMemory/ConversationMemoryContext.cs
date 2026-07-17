namespace EnterpriseAiDocumentAssistant.Api.ConversationMemory;

public sealed record ConversationMemoryContext(
    IReadOnlyList<ConversationMemoryTurn> Turns,
    string PromptText);

public sealed record ConversationMemoryTurn(
    string Role,
    string Content);
