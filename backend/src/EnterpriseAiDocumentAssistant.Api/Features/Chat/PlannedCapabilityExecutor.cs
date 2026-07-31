using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.Planner;
using EnterpriseAiDocumentAssistant.Api.PromptOrchestration;
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
            "skills.summary" => ConvertSummaryToAssistantMessage(await summarySkill.RunAsync(
                new SummarySkillRequest(plan.DocumentId, request.AiProvider, request.Message),
                cancellationToken), request),
            "skills.risk-analysis" => ConvertRiskAnalysisToAssistantMessage(await riskAnalysisSkill.RunAsync(
                new RiskAnalysisSkillRequest(plan.DocumentId, request.AiProvider, request.Message),
                cancellationToken), request),
            "skills.email-draft" => ConvertEmailDraftToAssistantMessage(await emailDraftSkill.RunAsync(
                new EmailDraftSkillRequest(plan.DocumentId, "Prepare a concise follow-up email draft.", request.AiProvider),
                cancellationToken)),
            "skills.classification" => ConvertClassificationToAssistantMessage(await classificationSkill.RunAsync(
                new ClassificationSkillRequest(plan.DocumentId, request.AiProvider),
                cancellationToken), request),
            "skills.resume-review" => ConvertResumeReviewToAssistantMessage(await resumeReviewSkill.RunAsync(
                new ResumeReviewSkillRequest(plan.DocumentId, request.Message, request.AiProvider),
                cancellationToken)),
            "tools.execute" => await toolCallingService.ExecuteSingleToolCallAsync(
                request,
                cancellationToken),
            "workflows.document-review" => ConvertWorkflowToAssistantMessage(await documentReviewWorkflow.RunAsync(
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

    private static StructuredAssistantMessage? ConvertSummaryToAssistantMessage(
        SummarySkillResponse? response,
        ChatRequest request)
    {
        return response is null
            ? null
            : new StructuredAssistantMessage(
                response.Summary,
                "high",
                response.Sources,
                BuildSummarySuggestedActions(request));
    }

    private static StructuredAssistantMessage? ConvertRiskAnalysisToAssistantMessage(
        RiskAnalysisSkillResponse? response,
        ChatRequest request)
    {
        if (response is null)
        {
            return null;
        }

        var answer = response.Risks.Count == 0
            ? BuildNoRisksMessage(request)
            : string.Join(Environment.NewLine, response.Risks.Select(risk =>
                $"- {risk.Title} ({FormatSeverity(risk.Severity, request)}): {risk.Recommendation}"));

        return new StructuredAssistantMessage(
            answer,
            "high",
            response.Risks.Select(risk => risk.Source).ToArray(),
            BuildRiskSuggestedActions(request));
    }

    private static StructuredAssistantMessage? ConvertEmailDraftToAssistantMessage(EmailDraftSkillResponse? response)
    {
        return response is null
            ? null
            : new StructuredAssistantMessage(
                $"{response.Subject}{Environment.NewLine}{Environment.NewLine}{response.Body}",
                "high",
                response.BasedOn,
                response.NextActions);
    }

    private static StructuredAssistantMessage? ConvertClassificationToAssistantMessage(
        ClassificationSkillResponse? response,
        ChatRequest request)
    {
        return response is null
            ? null
            : new StructuredAssistantMessage(
                $"Category: {response.Category}. Priority: {response.Priority}. {response.Reason}",
                response.Confidence >= 0.75 ? "high" : "medium",
                response.Sources,
                BuildClassificationSuggestedActions(request));
    }

    private static StructuredAssistantMessage? ConvertResumeReviewToAssistantMessage(ResumeReviewSkillResponse? response)
    {
        return response is null
            ? null
            : new StructuredAssistantMessage(
                response.Content,
                "high",
                response.BasedOn,
                response.NextActions);
    }

    private static StructuredAssistantMessage? ConvertWorkflowToAssistantMessage(
        DocumentReviewWorkflowResponse? response,
        ChatRequest request)
    {
        if (response is null)
        {
            return null;
        }

        var risks = response.RiskAnalysis.Risks.Count == 0
            ? BuildNoRisksMessage(request)
            : string.Join("; ", response.RiskAnalysis.Risks.Select(risk => $"{risk.Title} ({risk.Severity})"));

        var answer = EnterpriseAssistantPromptDefaults.PrefersChinese(request.Message)
            ? $"""
              流程已完成。

              摘要：{response.Summary.Summary}

              风险：{risks}

              邮件草稿：{response.EmailDraft.Subject}
              {response.EmailDraft.Body}
              """
            : $"""
              Workflow completed.

              Summary: {response.Summary.Summary}

              Risks: {risks}

              Email draft: {response.EmailDraft.Subject}
              {response.EmailDraft.Body}
              """;

        return new StructuredAssistantMessage(
            answer,
            "high",
            response.Summary.Sources
                .Concat(response.RiskAnalysis.Risks.Select(risk => risk.Source))
                .Concat(response.EmailDraft.BasedOn)
                .Distinct()
                .ToArray(),
            BuildWorkflowSuggestedActions(request));
    }

    private static IReadOnlyList<string> BuildSummarySuggestedActions(ChatRequest request)
    {
        return EnterpriseAssistantPromptDefaults.PrefersChinese(request.Message)
            ?
            [
                "分析风险",
                "生成后续邮件",
                "检查简历定位"
            ]
            :
            [
                "Analyze risks",
                "Generate follow-up email",
                "Review resume positioning"
            ];
    }

    private static IReadOnlyList<string> BuildRiskSuggestedActions(ChatRequest request)
    {
        return EnterpriseAssistantPromptDefaults.PrefersChinese(request.Message)
            ?
            [
                "总结关键点",
                "生成后续邮件",
                "执行完整流程"
            ]
            :
            [
                "Summarize key points",
                "Generate follow-up email",
                "Run full workflow"
            ];
    }

    private static IReadOnlyList<string> BuildClassificationSuggestedActions(ChatRequest request)
    {
        return EnterpriseAssistantPromptDefaults.PrefersChinese(request.Message)
            ?
            [
                "总结这份文档",
                "分析风险",
                "生成简历评估"
            ]
            :
            [
                "Summarize this document",
                "Analyze risks",
                "Generate resume review"
            ];
    }

    private static IReadOnlyList<string> BuildWorkflowSuggestedActions(ChatRequest request)
    {
        return EnterpriseAssistantPromptDefaults.PrefersChinese(request.Message)
            ?
            [
                "查看引用来源",
                "优化邮件草稿",
                "继续追问"
            ]
            :
            [
                "Review citations",
                "Refine email draft",
                "Ask a follow-up question"
            ];
    }

    private static string BuildNoRisksMessage(ChatRequest request)
    {
        return EnterpriseAssistantPromptDefaults.PrefersChinese(request.Message)
            ? "从当前文档上下文中没有识别出明显风险项。"
            : "No major risk items were identified from the selected document.";
    }

    private static string FormatSeverity(string severity, ChatRequest request)
    {
        if (!EnterpriseAssistantPromptDefaults.PrefersChinese(request.Message))
        {
            return severity;
        }

        return severity.ToLowerInvariant() switch
        {
            "high" => "高",
            "low" => "低",
            _ => "中"
        };
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength
            ? value
            : $"{value[..maxLength]}...";
    }
}
