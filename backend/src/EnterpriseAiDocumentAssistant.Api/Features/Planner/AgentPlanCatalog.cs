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
            "document_review_workflow" => "workflows.document-review",
            "resume_review" => "skills.resume-review",
            "classification" => "skills.classification",
            "email_draft" => "skills.email-draft",
            "risk_analysis" => "skills.risk-analysis",
            "summary" => "skills.summary",
            "tool_request" => "tools.execute",
            _ => "chat"
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
                "tool_request",
                "tools.execute",
                normalizedDocumentId,
                ["Select registered tool", "Validate arguments", "Execute through Tool Gateway"],
                ["GetHealthStatusTool", "GetDocumentMetadataTool"]),
            _ => new AgentPlanResponse(
                "document_question",
                "chat",
                normalizedDocumentId,
                ["Retrieve relevant chunks", "Build prompt with conversation memory", "Generate grounded answer"],
                ["RAG", "PromptOrchestration", "ConversationMemory"])
        };
    }
}
