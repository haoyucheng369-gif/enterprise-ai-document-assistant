namespace EnterpriseAiDocumentAssistant.Api.Skills;

public sealed record ResumeReviewSkillRequest(
    string DocumentId,
    string Instruction,
    string? AiProvider = null);

public sealed record ResumeReviewSkillResponse(
    string DocumentId,
    string Title,
    string Format,
    string Content,
    IReadOnlyList<string> BasedOn,
    IReadOnlyList<string> NextActions,
    string Provider);
