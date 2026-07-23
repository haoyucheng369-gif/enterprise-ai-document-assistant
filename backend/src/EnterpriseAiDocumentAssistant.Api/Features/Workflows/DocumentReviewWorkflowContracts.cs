using EnterpriseAiDocumentAssistant.Api.Skills;

namespace EnterpriseAiDocumentAssistant.Api.Workflows;

public sealed record DocumentReviewWorkflowRequest(
    string DocumentId,
    string EmailPurpose,
    string? AiProvider = null);

public sealed record DocumentReviewWorkflowResponse(
    string WorkflowId,
    string Status,
    string DocumentId,
    IReadOnlyList<WorkflowStepResult> Steps,
    SummarySkillResponse Summary,
    RiskAnalysisSkillResponse RiskAnalysis,
    EmailDraftSkillResponse EmailDraft);

public sealed record WorkflowStepResult(
    string Name,
    string Status,
    string Detail);
