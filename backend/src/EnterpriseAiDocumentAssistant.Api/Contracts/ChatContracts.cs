namespace EnterpriseAiDocumentAssistant.Api.Contracts;

public sealed record ChatRequest(
    string Message,
    string? DocumentId,
    IReadOnlyList<MessageResponse> History);

public sealed record ChatResponse(
    MessageResponse Message);

public sealed record StructuredChatResponse(
    StructuredAssistantMessage Message);

public sealed record StructuredAssistantMessage(
    string Answer,
    string Confidence,
    IReadOnlyList<string> Citations,
    IReadOnlyList<string> SuggestedActions);
