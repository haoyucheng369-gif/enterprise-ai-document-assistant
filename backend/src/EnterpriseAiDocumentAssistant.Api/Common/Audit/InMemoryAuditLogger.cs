using System.Collections.Concurrent;
using EnterpriseAiDocumentAssistant.Api.Services;

namespace EnterpriseAiDocumentAssistant.Api.Audit;

public sealed class InMemoryAuditLogger : IAuditLogger
{
    private const int MaxEvents = 200;
    private readonly ConcurrentQueue<AuditEvent> events = new();
    private readonly ISystemClock systemClock;

    public InMemoryAuditLogger(ISystemClock systemClock)
    {
        this.systemClock = systemClock;
    }

    public void Record(AuditEventRequest request)
    {
        // Keep the first audit trail in memory so it is easy to inspect from Swagger.
        events.Enqueue(new AuditEvent(
            Guid.NewGuid().ToString("N"),
            systemClock.UtcNow,
            request.Category,
            request.Action,
            request.Route,
            request.Succeeded,
            request.DurationMs,
            request.Metadata ?? new Dictionary<string, string>()));

        while (events.Count > MaxEvents)
        {
            events.TryDequeue(out _);
        }
    }

    public IReadOnlyList<AuditEvent> ListRecent(int limit = 50)
    {
        var boundedLimit = Math.Clamp(limit, 1, MaxEvents);

        return events
            .Reverse()
            .Take(boundedLimit)
            .ToArray();
    }
}
