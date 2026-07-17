namespace EnterpriseAiDocumentAssistant.Api.Skills;

public sealed record EmailDraftSkillRequest(
    string DocumentId,
    string Purpose);

public sealed record EmailDraftSkillResponse(
    string DocumentId,
    string Subject,
    string Body,
    IReadOnlyList<string> BasedOn,
    IReadOnlyList<string> NextActions);
