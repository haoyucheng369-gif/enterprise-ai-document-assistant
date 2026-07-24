namespace EnterpriseAiDocumentAssistant.Api.Planner;

public interface IAgentPlanner
{
    AgentPlanResponse Plan(AgentPlanRequest request);

    Task<AgentPlanResponse> PlanAsync(
        AgentPlanRequest request,
        CancellationToken cancellationToken);
}
