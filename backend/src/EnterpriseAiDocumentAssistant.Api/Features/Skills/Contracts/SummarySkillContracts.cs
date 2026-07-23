namespace EnterpriseAiDocumentAssistant.Api.Skills;

public sealed record SummarySkillRequest(
    string DocumentId,
    string? AiProvider = null);

public sealed record SummarySkillResponse(
    string DocumentId,
    string Title,
    string Summary,
    IReadOnlyList<string> KeyPoints,
    IReadOnlyList<string> Sources);
