using System.Diagnostics;
using EnterpriseAiDocumentAssistant.Api.Audit;

namespace EnterpriseAiDocumentAssistant.Api.Planner;

public sealed class SimpleAgentPlanner : IAgentPlanner
{
    private readonly IAuditLogger auditLogger;

    public SimpleAgentPlanner(IAuditLogger auditLogger)
    {
        this.auditLogger = auditLogger;
    }

    public AgentPlanResponse Plan(AgentPlanRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        var plan = CreateFallbackPlan(request);

        auditLogger.Record(new AuditEventRequest(
            "planner",
            "plan_created",
            plan.Route,
            true,
            stopwatch.ElapsedMilliseconds,
            new Dictionary<string, string>
            {
                ["intent"] = plan.Intent,
                ["documentId"] = plan.DocumentId
            }));

        return plan;
    }

    public Task<AgentPlanResponse> PlanAsync(
        AgentPlanRequest request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Plan(request));
    }

    private static AgentPlanResponse CreateFallbackPlan(AgentPlanRequest request)
    {
        var message = request.Message.Trim();

        // Deterministic rules provide the fallback route when AI routing is unavailable or invalid.
        if (ContainsAny(message, "workflow", "review document", "full review", "\u5de5\u4f5c\u6d41", "\u5b8c\u6574\u5206\u6790"))
        {
            return AgentPlanCatalog.Create("workflows.document-review", request.DocumentId);
        }

        if (ContainsAny(message, "resume", "cv", "career", "\u7b80\u5386", "\u5c65\u5386"))
        {
            return AgentPlanCatalog.Create("skills.resume-review", request.DocumentId);
        }

        if (ContainsAny(message, "classify", "classification", "category", "what is this document", "\u5206\u7c7b", "\u662f\u4ec0\u4e48"))
        {
            return AgentPlanCatalog.Create("skills.classification", request.DocumentId);
        }

        if (ContainsAny(message, "email", "mail", "draft", "\u90ae\u4ef6", "\u8349\u7a3f"))
        {
            return AgentPlanCatalog.Create("skills.email-draft", request.DocumentId);
        }

        if (ContainsAny(message, "risk", "risks", "liability", "\u98ce\u9669", "\u8d23\u4efb"))
        {
            return AgentPlanCatalog.Create("skills.risk-analysis", request.DocumentId);
        }

        if (ContainsAny(message, "summary", "summarize", "overview", "\u603b\u7ed3", "\u6982\u62ec"))
        {
            return AgentPlanCatalog.Create("skills.summary", request.DocumentId);
        }

        if (ContainsAny(message, "status", "health", "metadata", "\u72b6\u6001", "\u5143\u6570\u636e"))
        {
            return AgentPlanCatalog.Create("tools.execute", request.DocumentId);
        }

        return AgentPlanCatalog.Create("chat", request.DocumentId);
    }

    private static bool ContainsAny(string value, params string[] keywords)
    {
        return keywords.Any(keyword => value.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
}
