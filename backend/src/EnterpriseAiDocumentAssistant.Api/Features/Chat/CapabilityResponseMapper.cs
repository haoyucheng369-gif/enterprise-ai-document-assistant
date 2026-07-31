using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.PromptOrchestration;
using EnterpriseAiDocumentAssistant.Api.Skills;
using EnterpriseAiDocumentAssistant.Api.Workflows;

namespace EnterpriseAiDocumentAssistant.Api.Chat;

internal static class CapabilityResponseMapper
{
    public static StructuredAssistantMessage? FromSummary(
        SummarySkillResponse? response,
        ChatRequest request)
    {
        return response is null
            ? null
            : new StructuredAssistantMessage(
                response.Summary,
                "high",
                response.Sources,
                PrefersChinese(request) ? ["分析风险", "生成后续邮件", "检查简历定位"] :
                    ["Analyze risks", "Generate follow-up email", "Review resume positioning"]);
    }

    public static StructuredAssistantMessage? FromRiskAnalysis(
        RiskAnalysisSkillResponse? response,
        ChatRequest request)
    {
        if (response is null)
        {
            return null;
        }

        var answer = response.Risks.Count == 0
            ? NoRisksMessage(request)
            : string.Join(Environment.NewLine, response.Risks.Select(risk =>
                $"- {risk.Title} ({FormatSeverity(risk.Severity, request)}): {risk.Recommendation}"));

        return new StructuredAssistantMessage(
            answer,
            "high",
            response.Risks.Select(risk => risk.Source).ToArray(),
            PrefersChinese(request) ? ["总结关键点", "生成后续邮件", "执行完整流程"] :
                ["Summarize key points", "Generate follow-up email", "Run full workflow"]);
    }

    public static StructuredAssistantMessage? FromEmailDraft(EmailDraftSkillResponse? response)
    {
        return response is null
            ? null
            : new StructuredAssistantMessage(
                $"{response.Subject}{Environment.NewLine}{Environment.NewLine}{response.Body}",
                "high",
                response.BasedOn,
                response.NextActions);
    }

    public static StructuredAssistantMessage? FromClassification(
        ClassificationSkillResponse? response,
        ChatRequest request)
    {
        return response is null
            ? null
            : new StructuredAssistantMessage(
                $"Category: {response.Category}. Priority: {response.Priority}. {response.Reason}",
                response.Confidence >= 0.75 ? "high" : "medium",
                response.Sources,
                PrefersChinese(request) ? ["总结这份文档", "分析风险", "生成简历评估"] :
                    ["Summarize this document", "Analyze risks", "Generate resume review"]);
    }

    public static StructuredAssistantMessage? FromResumeReview(ResumeReviewSkillResponse? response)
    {
        return response is null
            ? null
            : new StructuredAssistantMessage(
                response.Content,
                "high",
                response.BasedOn,
                response.NextActions);
    }

    public static StructuredAssistantMessage? FromWorkflow(
        DocumentReviewWorkflowResponse? response,
        ChatRequest request)
    {
        if (response is null)
        {
            return null;
        }

        var risks = response.RiskAnalysis.Risks.Count == 0
            ? NoRisksMessage(request)
            : string.Join("; ", response.RiskAnalysis.Risks.Select(risk => $"{risk.Title} ({risk.Severity})"));

        var answer = PrefersChinese(request)
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
            PrefersChinese(request) ? ["查看引用来源", "优化邮件草稿", "继续追问"] :
                ["Review citations", "Refine email draft", "Ask a follow-up question"]);
    }

    private static bool PrefersChinese(ChatRequest request)
    {
        return EnterpriseAssistantPromptDefaults.PrefersChinese(request.Message);
    }

    private static string NoRisksMessage(ChatRequest request)
    {
        return PrefersChinese(request)
            ? "从当前文档上下文中没有识别出明显风险项。"
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
            "high" => "高",
            "low" => "低",
            _ => "中"
        };
    }
}
