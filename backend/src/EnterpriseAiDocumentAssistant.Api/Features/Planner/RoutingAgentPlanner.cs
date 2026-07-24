using EnterpriseAiDocumentAssistant.Api.Audit;
using System.Text.Json;

namespace EnterpriseAiDocumentAssistant.Api.Planner;

public sealed class RoutingAgentPlanner : IAgentPlanner
{
    private readonly IAuditLogger auditLogger;
    private readonly AiAgentPlanner aiAgentPlanner;
    private readonly SimpleAgentPlanner fallbackPlanner;

    public RoutingAgentPlanner(
        AiAgentPlanner aiAgentPlanner,
        SimpleAgentPlanner fallbackPlanner,
        IAuditLogger auditLogger)
    {
        this.aiAgentPlanner = aiAgentPlanner;
        this.fallbackPlanner = fallbackPlanner;
        this.auditLogger = auditLogger;
    }

    public AgentPlanResponse Plan(AgentPlanRequest request)
    {
        return fallbackPlanner.Plan(request);
    }

    public async Task<AgentPlanResponse> PlanAsync(
        AgentPlanRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // AI routing is preferred for free-form user language because it can understand intent beyond keywords.
            var aiPlan = await aiAgentPlanner.TryPlanAsync(request, cancellationToken);
            if (aiPlan is not null)
            {
                Record("ai_plan_selected", aiPlan, true);
                return aiPlan;
            }
        }
        catch (Exception ex) when (ex is InvalidOperationException or HttpRequestException or TaskCanceledException or JsonException)
        {
            RecordFallback(request, $"ai_planner_failed:{ex.GetType().Name}");
        }

        // Fallback keeps the assistant usable when the model is unavailable or returns an invalid route.
        var fallbackPlan = fallbackPlanner.Plan(request);
        Record("fallback_plan_selected", fallbackPlan, true);
        return fallbackPlan;
    }

    private void RecordFallback(AgentPlanRequest request, string reason)
    {
        auditLogger.Record(new AuditEventRequest(
            "planner",
            "ai_plan_fallback",
            "planner.routing",
            false,
            0,
            new Dictionary<string, string>
            {
                ["documentId"] = request.DocumentId ?? string.Empty,
                ["reason"] = reason
            }));
    }

    private void Record(string action, AgentPlanResponse plan, bool succeeded)
    {
        auditLogger.Record(new AuditEventRequest(
            "planner",
            action,
            plan.Route,
            succeeded,
            0,
            new Dictionary<string, string>
            {
                ["intent"] = plan.Intent,
                ["documentId"] = plan.DocumentId
            }));
    }
}
