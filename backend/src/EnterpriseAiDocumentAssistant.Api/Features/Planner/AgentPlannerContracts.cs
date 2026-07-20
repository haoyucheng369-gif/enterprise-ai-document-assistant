namespace EnterpriseAiDocumentAssistant.Api.Planner;

public sealed record AgentPlanRequest(
    string Message,
    string? DocumentId);

public sealed record AgentPlanResponse(
    string Intent,
    string Route,
    string DocumentId,
    IReadOnlyList<string> Steps,
    IReadOnlyList<string> Capabilities);
