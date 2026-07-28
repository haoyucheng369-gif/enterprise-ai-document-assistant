using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.Planner;
using EnterpriseAiDocumentAssistant.Api.Services;
using EnterpriseAiDocumentAssistant.Api.Skills;
using EnterpriseAiDocumentAssistant.Api.Workflows;

namespace EnterpriseAiDocumentAssistant.Api.Chat;

public sealed class AssistantMessageAdapter : IAssistantMessageAdapter
{
    private readonly IApplicationDocumentProvider applicationDocumentProvider;
    private readonly ISummarySkill summarySkill;
    private readonly IRiskAnalysisSkill riskAnalysisSkill;
    private readonly IEmailDraftSkill emailDraftSkill;
    private readonly IClassificationSkill classificationSkill;
    private readonly IResumeReviewSkill resumeReviewSkill;
    private readonly IDocumentReviewWorkflow documentReviewWorkflow;

    public AssistantMessageAdapter(
        IApplicationDocumentProvider applicationDocumentProvider,
        ISummarySkill summarySkill,
        IRiskAnalysisSkill riskAnalysisSkill,
        IEmailDraftSkill emailDraftSkill,
        IClassificationSkill classificationSkill,
        IResumeReviewSkill resumeReviewSkill,
        IDocumentReviewWorkflow documentReviewWorkflow)
    {
        this.applicationDocumentProvider = applicationDocumentProvider;
        this.summarySkill = summarySkill;
        this.riskAnalysisSkill = riskAnalysisSkill;
        this.emailDraftSkill = emailDraftSkill;
        this.classificationSkill = classificationSkill;
        this.resumeReviewSkill = resumeReviewSkill;
        this.documentReviewWorkflow = documentReviewWorkflow;
    }

    public async Task<StructuredAssistantMessage?> TryBuildFromPlanAsync(
        ChatRequest request,
        AgentPlanResponse plan,
        CancellationToken cancellationToken)
    {
        // Planner routes become executable application capabilities here; chat orchestration stays route-agnostic.
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
        // V1 citations come from parsed sections; the later RAG step can replace this with retrieved chunks.
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

        var answer = PrefersChinese(request)
            ? $"""
              \u6d41\u7a0b\u5df2\u5b8c\u6210\u3002

              \u6458\u8981\uff1a{response.Summary.Summary}

              \u98ce\u9669\uff1a{risks}

              \u90ae\u4ef6\u8349\u7a3f\uff1a{response.EmailDraft.Subject}
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
        return PrefersChinese(request)
            ?
            [
                "\u5206\u6790\u98ce\u9669",
                "\u751f\u6210\u540e\u7eed\u90ae\u4ef6",
                "\u68c0\u67e5\u7b80\u5386\u5b9a\u4f4d"
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
        return PrefersChinese(request)
            ?
            [
                "\u603b\u7ed3\u5173\u952e\u70b9",
                "\u751f\u6210\u540e\u7eed\u90ae\u4ef6",
                "\u6267\u884c\u5b8c\u6574\u6d41\u7a0b"
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
        return PrefersChinese(request)
            ?
            [
                "\u603b\u7ed3\u8fd9\u4efd\u6587\u6863",
                "\u5206\u6790\u98ce\u9669",
                "\u751f\u6210\u7b80\u5386\u8bc4\u4f30"
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
        return PrefersChinese(request)
            ?
            [
                "\u67e5\u770b\u5f15\u7528\u6765\u6e90",
                "\u4f18\u5316\u90ae\u4ef6\u8349\u7a3f",
                "\u7ee7\u7eed\u8ffd\u95ee"
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
        return PrefersChinese(request)
            ? "\u4ece\u5f53\u524d\u6587\u6863\u4e0a\u4e0b\u6587\u4e2d\u6ca1\u6709\u8bc6\u522b\u51fa\u660e\u663e\u98ce\u9669\u9879\u3002"
            : "No major risk items were identified from the selected document.";
    }

    private static string FormatSeverity(string severity, ChatRequest request)
    {
        if (!PrefersChinese(request))
        {
            return severity;
        }

        return severity.ToLowerInvariant() switch
        {
            "high" => "\u9ad8",
            "low" => "\u4f4e",
            _ => "\u4e2d"
        };
    }

    private static bool PrefersChinese(ChatRequest request)
    {
        return request.Message.Any(character => character is >= '\u4e00' and <= '\u9fff');
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength
            ? value
            : $"{value[..maxLength]}...";
    }
}
