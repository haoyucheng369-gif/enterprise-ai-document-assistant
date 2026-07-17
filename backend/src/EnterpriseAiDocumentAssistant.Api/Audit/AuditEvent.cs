namespace EnterpriseAiDocumentAssistant.Api.Audit;

public sealed record AuditEvent(
    string Id,
    DateTimeOffset Timestamp,
    string Category,
    string Action,
    string Route,
    bool Succeeded,
    long DurationMs,
    IReadOnlyDictionary<string, string> Metadata);
