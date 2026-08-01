using EnterpriseAiDocumentAssistant.Api.Skills;

namespace EnterpriseAiDocumentAssistant.Api.Agents;

public sealed record DocumentAgentRequest(
    string DocumentId,
    string EmailPurpose,
    string? AiProvider = null);

// The handoff is the stable contract passed from document analysis to email composition.
public sealed record DocumentAgentHandoff(
    string HandoffId,
    string DocumentId,
    string EmailPurpose,
    string? AiProvider,
    SummarySkillResponse Summary,
    RiskAnalysisSkillResponse RiskAnalysis);
