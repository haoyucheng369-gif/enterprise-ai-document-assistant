namespace EnterpriseAiDocumentAssistant.Api.Planner;

public interface IAgentPlanner
{
    AgentPlanResponse Plan(AgentPlanRequest request);
}
