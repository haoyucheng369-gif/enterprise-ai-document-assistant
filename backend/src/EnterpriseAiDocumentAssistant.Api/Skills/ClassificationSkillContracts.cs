namespace EnterpriseAiDocumentAssistant.Api.Skills;

public sealed record ClassificationSkillRequest(
    string DocumentId,
    string? AiProvider = null);

public sealed record ClassificationSkillResponse(
    string DocumentId,
    string Title,
    string Category,
    string Priority,
    double Confidence,
    string Reason,
    IReadOnlyList<string> Signals,
    IReadOnlyList<string> Sources,
    string Provider);
