namespace EnterpriseAiDocumentAssistant.Api.Contracts;

public sealed record ChatRequest(
    string Message,
    string? DocumentId,
    IReadOnlyList<MessageResponse> History,
    string? AiProvider = null);

public sealed record ChatResponse(
    MessageResponse Message);

public sealed record StructuredChatResponse(
    StructuredAssistantMessage Message);

public sealed record StructuredAssistantMessage(
    string Answer,
    string Confidence,
    IReadOnlyList<string> Citations,
    IReadOnlyList<string> SuggestedActions);
