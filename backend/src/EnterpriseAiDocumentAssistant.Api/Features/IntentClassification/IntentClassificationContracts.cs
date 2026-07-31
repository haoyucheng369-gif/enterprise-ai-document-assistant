namespace EnterpriseAiDocumentAssistant.Api.IntentClassification;

public sealed record IntentClassificationRequest(
    string Message,
    string? DocumentId,
    string? AiProvider = null);

public sealed record IntentClassificationResult(
    string Intent,
    string Reason,
    string Source);
