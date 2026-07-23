namespace EnterpriseAiDocumentAssistant.Api.Skills;

public sealed record EmailDraftSkillRequest(
    string DocumentId,
    string Purpose,
    string? AiProvider = null);

public sealed record EmailDraftSkillResponse(
    string DocumentId,
    string Subject,
    string Body,
    IReadOnlyList<string> BasedOn,
    IReadOnlyList<string> NextActions);
