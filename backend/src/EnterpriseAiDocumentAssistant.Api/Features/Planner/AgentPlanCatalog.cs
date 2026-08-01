namespace EnterpriseAiDocumentAssistant.Api.Planner;

internal static class AgentPlanCatalog
{
    public static readonly IReadOnlyList<string> Intents =
    [
        "document_question",
        "summary",
        "risk_analysis",
        "email_draft",
        "classification",
        "resume_review",
        "tool_request",
        "document_review_workflow"
    ];

    public static bool IsKnownIntent(string intent)
    {
        return Intents.Contains(intent, StringComparer.OrdinalIgnoreCase);
    }

    public static AgentPlanResponse CreateFromIntent(string intent, string? documentId)
    {
        // Planner owns the stable mapping from classified business intent to executable route.
        var route = intent.Trim().ToLowerInvariant() switch
        {
            "document_review_workflow" => AgentPlanRoutes.DocumentReviewWorkflow,
            "resume_review" => AgentPlanRoutes.ResumeReview,
            "classification" => AgentPlanRoutes.Classification,
            "email_draft" => AgentPlanRoutes.EmailDraft,
            "risk_analysis" => AgentPlanRoutes.RiskAnalysis,
            "summary" => AgentPlanRoutes.Summary,
            "tool_request" => AgentPlanRoutes.ToolExecution,
            _ => AgentPlanRoutes.Chat
        };

        return Create(route, documentId);
    }

    public static AgentPlanResponse Create(string route, string? documentId)
    {
        var normalizedDocumentId = string.IsNullOrWhiteSpace(documentId)
            ? string.Empty
            : documentId.Trim();

        return route.Trim().ToLowerInvariant() switch
        {
            AgentPlanRoutes.DocumentReviewWorkflow => new AgentPlanResponse(
                "document_review_workflow",
                AgentPlanRoutes.DocumentReviewWorkflow,
                normalizedDocumentId,
                ["Summarize document", "Analyze risks", "Draft follow-up email"],
                ["DocumentAgent", "EmailAgent"]),
            AgentPlanRoutes.ResumeReview => new AgentPlanResponse(
                "resume_review",
                AgentPlanRoutes.ResumeReview,
                normalizedDocumentId,
                ["Read selected resume", "Identify strengths and gaps", "Generate Markdown review brief"],
                ["ResumeReviewSkill"]),
            AgentPlanRoutes.Classification => new AgentPlanResponse(
                "classification",
                AgentPlanRoutes.Classification,
                normalizedDocumentId,
                ["Read selected document", "Classify business category", "Return priority and confidence"],
                ["ClassificationSkill"]),
            AgentPlanRoutes.EmailDraft => new AgentPlanResponse(
                "email_draft",
                AgentPlanRoutes.EmailDraft,
                normalizedDocumentId,
                ["Read selected document", "Summarize document", "Analyze risks", "Draft follow-up email"],
                ["SummarySkill", "RiskAnalysisSkill", "EmailDraftSkill"]),
            AgentPlanRoutes.RiskAnalysis => new AgentPlanResponse(
                "risk_analysis",
                AgentPlanRoutes.RiskAnalysis,
                normalizedDocumentId,
                ["Read selected document", "Identify risk signals", "Return severity and recommendations"],
                ["RiskAnalysisSkill"]),
            AgentPlanRoutes.Summary => new AgentPlanResponse(
                "summary",
                AgentPlanRoutes.Summary,
                normalizedDocumentId,
                ["Read selected document", "Extract key points", "Return structured summary"],
                ["SummarySkill"]),
            AgentPlanRoutes.ToolExecution => new AgentPlanResponse(
                "tool_request",
                AgentPlanRoutes.ToolExecution,
                normalizedDocumentId,
                ["Select registered tool", "Validate arguments", "Execute through Tool Gateway"],
                ["GetHealthStatusTool", "GetDocumentMetadataTool"]),
            _ => new AgentPlanResponse(
                "document_question",
                AgentPlanRoutes.Chat,
                normalizedDocumentId,
                ["Retrieve relevant chunks", "Build prompt with conversation memory", "Generate grounded answer"],
                ["RAG", "PromptOrchestration", "ConversationMemory"])
        };
    }
}
