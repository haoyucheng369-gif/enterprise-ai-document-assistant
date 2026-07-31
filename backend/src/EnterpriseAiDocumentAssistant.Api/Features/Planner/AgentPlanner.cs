using EnterpriseAiDocumentAssistant.Api.Audit;
using EnterpriseAiDocumentAssistant.Api.IntentClassification;

namespace EnterpriseAiDocumentAssistant.Api.Planner;

public sealed class AgentPlanner : IAgentPlanner
{
    private readonly IIntentClassifier intentClassifier;
    private readonly IAuditLogger auditLogger;

    public AgentPlanner(
        IIntentClassifier intentClassifier,
        IAuditLogger auditLogger)
    {
        this.intentClassifier = intentClassifier;
        this.auditLogger = auditLogger;
    }

    public AgentPlanResponse Plan(AgentPlanRequest request)
    {
        var classification = intentClassifier.Classify(ToClassificationRequest(request));
        return CreateAndRecordPlan(classification, request.DocumentId);
    }

    public async Task<AgentPlanResponse> PlanAsync(
        AgentPlanRequest request,
        CancellationToken cancellationToken)
    {
        // Planner consumes a classified intent and converts it into a controlled route and known steps.
        var classification = await intentClassifier.ClassifyAsync(
            ToClassificationRequest(request),
            cancellationToken);

        return CreateAndRecordPlan(classification, request.DocumentId);
    }

    private AgentPlanResponse CreateAndRecordPlan(
        IntentClassificationResult classification,
        string? documentId)
    {
        var plan = AgentPlanCatalog.CreateFromIntent(classification.Intent, documentId);

        auditLogger.Record(new AuditEventRequest(
            "planner",
            "plan_created",
            plan.Route,
            true,
            0,
            new Dictionary<string, string>
            {
                ["intent"] = classification.Intent,
                ["classificationSource"] = classification.Source,
                ["documentId"] = plan.DocumentId
            }));

        return plan;
    }

    private static IntentClassificationRequest ToClassificationRequest(AgentPlanRequest request)
    {
        return new IntentClassificationRequest(
            request.Message,
            request.DocumentId,
            request.AiProvider);
    }
}
