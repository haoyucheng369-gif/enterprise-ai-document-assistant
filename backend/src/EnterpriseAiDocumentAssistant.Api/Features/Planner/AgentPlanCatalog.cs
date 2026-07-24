namespace EnterpriseAiDocumentAssistant.Api.Planner;

internal static class AgentPlanCatalog
{
    private const string DefaultDocumentId = "contract-review";

    public static readonly IReadOnlyList<string> Routes =
    [
        "chat",
        "skills.summary",
        "skills.risk-analysis",
        "skills.email-draft",
        "skills.classification",
        "skills.resume-review",
        "tools.execute",
        "workflows.document-review"
    ];

    public static bool IsKnownRoute(string route)
    {
        return Routes.Contains(route, StringComparer.OrdinalIgnoreCase);
    }

    public static AgentPlanResponse Create(string route, string? documentId)
    {
        var normalizedDocumentId = string.IsNullOrWhiteSpace(documentId)
            ? DefaultDocumentId
            : documentId.Trim();

        return route.Trim().ToLowerInvariant() switch
        {
            "workflows.document-review" => new AgentPlanResponse(
                "document_review_workflow",
                "workflows.document-review",
                normalizedDocumentId,
                ["Summarize document", "Analyze risks", "Draft follow-up email"],
                ["SummarySkill", "RiskAnalysisSkill", "EmailDraftSkill"]),
            "skills.resume-review" => new AgentPlanResponse(
                "resume_review",
                "skills.resume-review",
                normalizedDocumentId,
                ["Read selected resume", "Identify strengths and gaps", "Generate Markdown review brief"],
                ["ResumeReviewSkill"]),
            "skills.classification" => new AgentPlanResponse(
                "classification",
                "skills.classification",
                normalizedDocumentId,
                ["Read selected document", "Classify business category", "Return priority and confidence"],
                ["ClassificationSkill"]),
            "skills.email-draft" => new AgentPlanResponse(
                "email_draft",
                "skills.email-draft",
                normalizedDocumentId,
                ["Read selected document", "Summarize document", "Analyze risks", "Draft follow-up email"],
                ["SummarySkill", "RiskAnalysisSkill", "EmailDraftSkill"]),
            "skills.risk-analysis" => new AgentPlanResponse(
                "risk_analysis",
                "skills.risk-analysis",
                normalizedDocumentId,
                ["Read selected document", "Identify risk signals", "Return severity and recommendations"],
                ["RiskAnalysisSkill"]),
            "skills.summary" => new AgentPlanResponse(
                "summary",
                "skills.summary",
                normalizedDocumentId,
                ["Read selected document", "Extract key points", "Return structured summary"],
                ["SummarySkill"]),
            "tools.execute" => new AgentPlanResponse(
                "tool_lookup",
                "tools.execute",
                normalizedDocumentId,
                ["Select registered tool", "Validate arguments", "Execute through Tool Gateway"],
                ["GetHealthStatusTool", "GetDocumentMetadataTool"]),
            _ => new AgentPlanResponse(
                "document_question",
                "chat",
                normalizedDocumentId,
                ["Build prompt", "Use conversation memory", "Prepare answer path for later RAG"],
                ["PromptOrchestration", "ConversationMemory"])
        };
    }
}
