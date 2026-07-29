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
        // Keyword routing is the deterministic backup for local runs and invalid AI planner output.
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
        // Route order matters: only explicit application actions select a specialized capability.
        // Document facts, comparisons, calculations, and local questions intentionally fall through to RAG chat.
        var message = request.Message.Trim();

        // Deterministic rules provide the fallback route when AI routing is unavailable or invalid.
        if (ContainsAny(
            message,
            "run full workflow",
            "run document workflow",
            "complete document review",
            "执行完整流程",
            "运行完整流程",
            "完整审查文档"))
        {
            return AgentPlanCatalog.Create("workflows.document-review", request.DocumentId);
        }

        if (ContainsAny(
            message,
            "review this resume",
            "analyze this resume",
            "resume review",
            "improve this cv",
            "评估这份简历",
            "分析简历优缺点",
            "优化这份简历",
            "简历评审"))
        {
            return AgentPlanCatalog.Create("skills.resume-review", request.DocumentId);
        }

        if (ContainsAny(
            message,
            "classify this document",
            "classify the document",
            "document classification",
            "determine the document category",
            "给这份文档分类",
            "文档分类",
            "判断文档类型"))
        {
            return AgentPlanCatalog.Create("skills.classification", request.DocumentId);
        }

        if (ContainsAny(
            message,
            "draft an email",
            "generate an email",
            "write a follow-up email",
            "create email draft",
            "生成邮件",
            "起草邮件",
            "写一封邮件",
            "生成后续邮件"))
        {
            return AgentPlanCatalog.Create("skills.email-draft", request.DocumentId);
        }

        if (ContainsAny(
            message,
            "analyze the risks",
            "analyze risks",
            "identify risks",
            "risk assessment",
            "complete risk analysis",
            "分析这份文档的风险",
            "分析风险",
            "识别风险",
            "风险评估"))
        {
            return AgentPlanCatalog.Create("skills.risk-analysis", request.DocumentId);
        }

        if (ContainsAny(
            message,
            "summarize this document",
            "summarize the document",
            "summarize the entire document",
            "full document summary",
            "generate a document summary",
            "总结这份文档",
            "总结整份文档",
            "总结全文",
            "概括全文",
            "生成文档摘要"))
        {
            return AgentPlanCatalog.Create("skills.summary", request.DocumentId);
        }

        if (ContainsAny(
            message,
            "check system health",
            "get document metadata",
            "execute health status tool",
            "检查系统健康状态",
            "获取文档元数据"))
        {
            return AgentPlanCatalog.Create("tools.execute", request.DocumentId);
        }

        // RAG is the default document-assistant path, including factual questions and calculations.
        return AgentPlanCatalog.Create("chat", request.DocumentId);
    }

    private static bool ContainsAny(string value, params string[] keywords)
    {
        return keywords.Any(keyword => value.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
}
