using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.Planner;
using EnterpriseAiDocumentAssistant.Api.Services;
using EnterpriseAiDocumentAssistant.Api.Skills;
using EnterpriseAiDocumentAssistant.Api.ToolCalling;
using EnterpriseAiDocumentAssistant.Api.Workflows;

namespace EnterpriseAiDocumentAssistant.Api.Chat;

public sealed class PlannedCapabilityExecutor : IPlannedCapabilityExecutor
{
    private readonly IApplicationDocumentProvider applicationDocumentProvider;
    private readonly ISummarySkill summarySkill;
    private readonly IRiskAnalysisSkill riskAnalysisSkill;
    private readonly IEmailDraftSkill emailDraftSkill;
    private readonly IClassificationSkill classificationSkill;
    private readonly IResumeReviewSkill resumeReviewSkill;
    private readonly IToolCallingService toolCallingService;
    private readonly IDocumentReviewWorkflow documentReviewWorkflow;

    public PlannedCapabilityExecutor(
        IApplicationDocumentProvider applicationDocumentProvider,
        ISummarySkill summarySkill,
        IRiskAnalysisSkill riskAnalysisSkill,
        IEmailDraftSkill emailDraftSkill,
        IClassificationSkill classificationSkill,
        IResumeReviewSkill resumeReviewSkill,
        IToolCallingService toolCallingService,
        IDocumentReviewWorkflow documentReviewWorkflow)
    {
        this.applicationDocumentProvider = applicationDocumentProvider;
        this.summarySkill = summarySkill;
        this.riskAnalysisSkill = riskAnalysisSkill;
        this.emailDraftSkill = emailDraftSkill;
        this.classificationSkill = classificationSkill;
        this.resumeReviewSkill = resumeReviewSkill;
        this.toolCallingService = toolCallingService;
        this.documentReviewWorkflow = documentReviewWorkflow;
    }

    public async Task<StructuredAssistantMessage?> ExecutePlanAsync(
        ChatRequest request,
        AgentPlanResponse plan,
        CancellationToken cancellationToken)
    {
        // Execute the capability selected by Planner, then adapt its typed result to one chat response.
        return plan.Route switch
        {
            AgentPlanRoutes.Summary => CapabilityResponseMapper.FromSummary(await summarySkill.RunAsync(
                new SummarySkillRequest(plan.DocumentId, request.AiProvider, request.Message),
                cancellationToken), request),
            AgentPlanRoutes.RiskAnalysis => CapabilityResponseMapper.FromRiskAnalysis(await riskAnalysisSkill.RunAsync(
                new RiskAnalysisSkillRequest(plan.DocumentId, request.AiProvider, request.Message),
                cancellationToken), request),
            AgentPlanRoutes.EmailDraft => CapabilityResponseMapper.FromEmailDraft(await emailDraftSkill.RunAsync(
                new EmailDraftSkillRequest(plan.DocumentId, "Prepare a concise follow-up email draft.", request.AiProvider),
                cancellationToken)),
            AgentPlanRoutes.Classification => CapabilityResponseMapper.FromClassification(await classificationSkill.RunAsync(
                new ClassificationSkillRequest(plan.DocumentId, request.AiProvider),
                cancellationToken), request),
            AgentPlanRoutes.ResumeReview => CapabilityResponseMapper.FromResumeReview(await resumeReviewSkill.RunAsync(
                new ResumeReviewSkillRequest(plan.DocumentId, request.Message, request.AiProvider),
                cancellationToken)),
            AgentPlanRoutes.ToolExecution => await toolCallingService.ExecuteSingleToolCallAsync(
                request,
                cancellationToken),
            AgentPlanRoutes.DocumentReviewWorkflow => CapabilityResponseMapper.FromWorkflow(await documentReviewWorkflow.RunAsync(
                new DocumentReviewWorkflowRequest(plan.DocumentId, "Prepare a concise follow-up email draft.", request.AiProvider),
                cancellationToken), request),
            _ => null
        };
    }

    public StructuredAssistantMessage AttachDocumentCitations(
        ChatRequest request,
        StructuredAssistantMessage message)
    {
        // Non-RAG capability responses use parsed sections as citations; RAG supplies retrieved citations directly.
        if (string.IsNullOrWhiteSpace(request.DocumentId))
        {
            return message;
        }

        var document = applicationDocumentProvider.FindById(request.DocumentId);
        if (document is null)
        {
            return message;
        }

        var citations = document.Sections
            .Take(4)
            .Select(section => $"{section.Label} - {section.Title}: {Truncate(section.Body, 120)}")
            .ToArray();

        return message with
        {
            Citations = citations.Length > 0
                ? citations
                : [$"Document: {document.Title}"]
        };
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength
            ? value
            : $"{value[..maxLength]}...";
    }
}
