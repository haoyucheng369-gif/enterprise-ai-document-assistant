namespace EnterpriseAiDocumentAssistant.Api.Audit;

public interface IAuditLogger
{
    void Record(AuditEventRequest request);

    IReadOnlyList<AuditEvent> ListRecent(int limit = 50);
}
