namespace EnterpriseAiDocumentAssistant.Api.Planner;

internal static class AgentPlanRoutes
{
    // Planner and executor share these route ids as one internal dispatch contract.
    public const string Chat = "chat";
    public const string Summary = "skills.summary";
    public const string RiskAnalysis = "skills.risk-analysis";
    public const string EmailDraft = "skills.email-draft";
    public const string Classification = "skills.classification";
    public const string ResumeReview = "skills.resume-review";
    public const string ToolExecution = "tools.execute";
    public const string DocumentReviewWorkflow = "workflows.document-review";
}
