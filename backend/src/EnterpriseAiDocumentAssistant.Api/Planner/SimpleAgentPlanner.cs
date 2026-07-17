namespace EnterpriseAiDocumentAssistant.Api.Planner;

public sealed class SimpleAgentPlanner : IAgentPlanner
{
    private const string DefaultDocumentId = "contract-review";

    public AgentPlanResponse Plan(AgentPlanRequest request)
    {
        var message = request.Message.Trim();
        var documentId = string.IsNullOrWhiteSpace(request.DocumentId)
            ? DefaultDocumentId
            : request.DocumentId.Trim();

        // The first planner uses deterministic rules so routing is easy to inspect and test.
        if (ContainsAny(message, "email", "mail", "draft", "邮件", "草稿"))
        {
            return CreatePlan(
                "email_draft",
                "skills.email-draft",
                documentId,
                ["Read selected document", "Summarize document", "Analyze risks", "Draft follow-up email"],
                ["SummarySkill", "RiskAnalysisSkill", "EmailDraftSkill"]);
        }

        if (ContainsAny(message, "risk", "risks", "liability", "风险", "责任"))
        {
            return CreatePlan(
                "risk_analysis",
                "skills.risk-analysis",
                documentId,
                ["Read selected document", "Identify risk signals", "Return severity and recommendations"],
                ["RiskAnalysisSkill"]);
        }

        if (ContainsAny(message, "summary", "summarize", "overview", "总结", "概括"))
        {
            return CreatePlan(
                "summary",
                "skills.summary",
                documentId,
                ["Read selected document", "Extract key points", "Return structured summary"],
                ["SummarySkill"]);
        }

        if (ContainsAny(message, "status", "health", "metadata", "元数据", "状态"))
        {
            return CreatePlan(
                "tool_lookup",
                "tools.execute",
                documentId,
                ["Select registered tool", "Validate arguments", "Execute through Tool Gateway"],
                ["GetHealthStatusTool", "GetDocumentMetadataTool"]);
        }

        return CreatePlan(
            "document_question",
            "chat",
            documentId,
            ["Build prompt", "Use conversation memory", "Prepare answer path for later RAG"],
            ["PromptOrchestration", "ConversationMemory"]);
    }

    private static AgentPlanResponse CreatePlan(
        string intent,
        string route,
        string documentId,
        IReadOnlyList<string> steps,
        IReadOnlyList<string> capabilities)
    {
        return new AgentPlanResponse(intent, route, documentId, steps, capabilities);
    }

    private static bool ContainsAny(string value, params string[] keywords)
    {
        return keywords.Any(keyword => value.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
}
