namespace EnterpriseAiDocumentAssistant.Api.Contracts;

public sealed record ChatRequest(
    string Message,
    string? DocumentId,
    IReadOnlyList<MessageResponse> History);

public sealed record ChatResponse(
    MessageResponse Message);
