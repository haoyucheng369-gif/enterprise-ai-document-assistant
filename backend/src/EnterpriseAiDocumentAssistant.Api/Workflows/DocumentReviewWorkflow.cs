using System.Diagnostics;
using EnterpriseAiDocumentAssistant.Api.Audit;
using EnterpriseAiDocumentAssistant.Api.Skills;

namespace EnterpriseAiDocumentAssistant.Api.Workflows;

public sealed class DocumentReviewWorkflow : IDocumentReviewWorkflow
{
    private readonly IAuditLogger auditLogger;
    private readonly IEmailDraftSkill emailDraftSkill;
    private readonly IRiskAnalysisSkill riskAnalysisSkill;
    private readonly ISummarySkill summarySkill;

    public DocumentReviewWorkflow(
        IAuditLogger auditLogger,
        ISummarySkill summarySkill,
        IRiskAnalysisSkill riskAnalysisSkill,
        IEmailDraftSkill emailDraftSkill)
    {
        this.auditLogger = auditLogger;
        this.summarySkill = summarySkill;
        this.riskAnalysisSkill = riskAnalysisSkill;
        this.emailDraftSkill = emailDraftSkill;
    }

    public DocumentReviewWorkflowResponse? Run(DocumentReviewWorkflowRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        var steps = new List<WorkflowStepResult>();

        // The first workflow is deterministic: run known skills in a fixed business sequence.
        var summary = summarySkill.Run(new SummarySkillRequest(request.DocumentId));
        if (summary is null)
        {
            RecordAudit(request.DocumentId, false, stopwatch.ElapsedMilliseconds, "summary_failed");
            return null;
        }

        steps.Add(new WorkflowStepResult(
            "SummarySkill",
            "Completed",
            "Document summary was generated."));

        var riskAnalysis = riskAnalysisSkill.Run(new RiskAnalysisSkillRequest(request.DocumentId));
        if (riskAnalysis is null)
        {
            RecordAudit(request.DocumentId, false, stopwatch.ElapsedMilliseconds, "risk_analysis_failed");
            return null;
        }

        steps.Add(new WorkflowStepResult(
            "RiskAnalysisSkill",
            "Completed",
            $"Risk analysis returned {riskAnalysis.Risks.Count} risk item(s)."));

        var emailDraft = emailDraftSkill.Run(new EmailDraftSkillRequest(
            request.DocumentId,
            request.EmailPurpose));
        if (emailDraft is null)
        {
            RecordAudit(request.DocumentId, false, stopwatch.ElapsedMilliseconds, "email_draft_failed");
            return null;
        }

        steps.Add(new WorkflowStepResult(
            "EmailDraftSkill",
            "Completed",
            "Follow-up email draft was generated."));

        RecordAudit(request.DocumentId, true, stopwatch.ElapsedMilliseconds, "completed");

        return new DocumentReviewWorkflowResponse(
            $"workflow-{Guid.NewGuid():N}",
            "Completed",
            request.DocumentId,
            steps,
            summary,
            riskAnalysis,
            emailDraft);
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
