using System.Diagnostics;
using EnterpriseAiDocumentAssistant.Api.Audit;
using EnterpriseAiDocumentAssistant.Api.Services;

namespace EnterpriseAiDocumentAssistant.Api.Integrations.MicrosoftGraph;

public sealed class MockMicrosoftGraphGateway : IMicrosoftGraphGateway
{
    private readonly IAuditLogger auditLogger;
    private readonly ISystemClock systemClock;

    public MockMicrosoftGraphGateway(
        IAuditLogger auditLogger,
        ISystemClock systemClock)
    {
        this.auditLogger = auditLogger;
        this.systemClock = systemClock;
    }

    public MicrosoftGraphEmailDraftResponse CreateEmailDraft(MicrosoftGraphEmailDraftRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        var draftId = $"graph-draft-{Guid.NewGuid():N}";

        // This mock keeps the enterprise integration boundary realistic without requiring OAuth yet.
        var response = new MicrosoftGraphEmailDraftResponse(
            draftId,
            "MicrosoftGraphMock",
            "DraftCreated",
            request.DocumentId,
            request.To.Trim(),
            request.Subject.Trim(),
            BuildPreview(request.Body),
            $"https://graph.microsoft.test/me/messages/{draftId}",
            systemClock.UtcNow);

        auditLogger.Record(new AuditEventRequest(
            "integration",
            "graph_email_draft_created",
            "integrations.graph.email-draft",
            true,
            stopwatch.ElapsedMilliseconds,
            new Dictionary<string, string>
            {
                ["documentId"] = request.DocumentId,
                ["draftId"] = draftId,
                ["provider"] = response.Provider
            }));

        return response;
    }

    private static string BuildPreview(string body)
    {
        var normalized = body.Trim();
        return normalized.Length <= 180
            ? normalized
            : $"{normalized[..180]}...";
    }
}
