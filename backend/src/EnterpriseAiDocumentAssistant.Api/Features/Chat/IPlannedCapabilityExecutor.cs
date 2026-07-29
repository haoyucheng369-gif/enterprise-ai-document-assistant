using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.Planner;

namespace EnterpriseAiDocumentAssistant.Api.Chat;

public interface IPlannedCapabilityExecutor
{
    Task<StructuredAssistantMessage?> ExecutePlanAsync(
        ChatRequest request,
        AgentPlanResponse plan,
        CancellationToken cancellationToken);

    StructuredAssistantMessage AttachDocumentCitations(
        ChatRequest request,
        StructuredAssistantMessage message);
}
