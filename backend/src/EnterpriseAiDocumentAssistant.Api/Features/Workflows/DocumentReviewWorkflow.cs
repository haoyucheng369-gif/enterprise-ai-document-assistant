using System.Diagnostics;
using EnterpriseAiDocumentAssistant.Api.Agents;
using EnterpriseAiDocumentAssistant.Api.Audit;
using EnterpriseAiDocumentAssistant.Api.Skills;

namespace EnterpriseAiDocumentAssistant.Api.Workflows;

public sealed class DocumentReviewWorkflow : IDocumentReviewWorkflow
{
    private readonly IAuditLogger auditLogger;
    private readonly IDocumentAgent documentAgent;
    private readonly IEmailAgent emailAgent;

    public DocumentReviewWorkflow(
        IAuditLogger auditLogger,
        IDocumentAgent documentAgent,
        IEmailAgent emailAgent)
    {
        this.auditLogger = auditLogger;
        this.documentAgent = documentAgent;
        this.emailAgent = emailAgent;
    }

    public DocumentReviewWorkflowResponse? Run(DocumentReviewWorkflowRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        var handoff = documentAgent.PrepareHandoff(ToAgentRequest(request));
        if (handoff is null)
        {
            RecordAudit(request.DocumentId, false, stopwatch.ElapsedMilliseconds, "document_agent_failed");
            return null;
        }

        var emailDraft = emailAgent.CreateDraft(handoff);
        return CompleteWorkflow(request.DocumentId, handoff, emailDraft, stopwatch);
    }

    public async Task<DocumentReviewWorkflowResponse?> RunAsync(
        DocumentReviewWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        // Agent Handoff Step 2: Workflow delegates document understanding to DocumentAgent.
        // DocumentAgent runs focused skills and returns one typed handoff object.
        var handoff = await documentAgent.PrepareHandoffAsync(
            ToAgentRequest(request),
            cancellationToken);
        if (handoff is null)
        {
            RecordAudit(request.DocumentId, false, stopwatch.ElapsedMilliseconds, "document_agent_failed");
            return null;
        }

        // Agent Handoff Step 3: pass the completed analysis to EmailAgent.
        // EmailAgent consumes the handoff; it does not rerun summary or risk analysis.
        var emailDraft = await emailAgent.CreateDraftAsync(handoff, cancellationToken);
        return CompleteWorkflow(request.DocumentId, handoff, emailDraft, stopwatch);
    }

    private DocumentReviewWorkflowResponse? CompleteWorkflow(
        string documentId,
        DocumentAgentHandoff handoff,
        EmailDraftSkillResponse? emailDraft,
        Stopwatch stopwatch)
    {
        // Agent Handoff Step 4: combine both agents' outputs into the existing workflow response.
        // The controller returns this response unchanged to the React workflow panel.
        if (emailDraft is null)
        {
            RecordAudit(documentId, false, stopwatch.ElapsedMilliseconds, "email_agent_failed");
            return null;
        }

        var steps = new WorkflowStepResult[]
        {
            new(
                "DocumentAgent",
                "Completed",
                $"Prepared handoff {handoff.HandoffId} with a summary and {handoff.RiskAnalysis.Risks.Count} risk item(s)."),
            new(
                "EmailAgent",
                "Completed",
                $"Consumed handoff {handoff.HandoffId} and generated the follow-up draft.")
        };

        RecordAudit(documentId, true, stopwatch.ElapsedMilliseconds, "completed");

        return new DocumentReviewWorkflowResponse(
            $"workflow-{Guid.NewGuid():N}",
            "Completed",
            documentId,
            steps,
            handoff.Summary,
            handoff.RiskAnalysis,
            emailDraft);
    }

    private static DocumentAgentRequest ToAgentRequest(DocumentReviewWorkflowRequest request)
    {
        return new DocumentAgentRequest(
            request.DocumentId,
            request.EmailPurpose,
            request.AiProvider);
    }

    private void RecordAudit(
        string documentId,
        bool succeeded,
        long durationMs,
        string result)
    {
        auditLogger.Record(new AuditEventRequest(
            "workflow",
            "document_review",
            "workflows.document-review",
            succeeded,
            durationMs,
            new Dictionary<string, string>
            {
                ["documentId"] = documentId,
                ["result"] = result
            }));
    }
}
