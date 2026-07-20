namespace EnterpriseAiDocumentAssistant.Api.Audit;

public sealed record AuditEventRequest(
    string Category,
    string Action,
    string Route,
    bool Succeeded,
    long DurationMs,
    IReadOnlyDictionary<string, string>? Metadata = null);
