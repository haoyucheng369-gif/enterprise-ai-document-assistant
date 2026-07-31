namespace EnterpriseAiDocumentAssistant.Api.Audit;

public sealed record AiExecutionResponse(
    string Id,
    DateTimeOffset Timestamp,
    string Operation,
    string Provider,
    string Model,
    string UserId,
    bool Succeeded,
    long DurationMs,
    int? InputTokens,
    int? OutputTokens);
