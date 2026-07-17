namespace EnterpriseAiDocumentAssistant.Api.Skills;

public sealed record RiskAnalysisSkillRequest(
    string DocumentId);

public sealed record RiskAnalysisSkillResponse(
    string DocumentId,
    string Title,
    IReadOnlyList<RiskItem> Risks,
    IReadOnlyList<string> MissingInformation);

public sealed record RiskItem(
    string Title,
    string Severity,
    string Source,
    string Recommendation);
