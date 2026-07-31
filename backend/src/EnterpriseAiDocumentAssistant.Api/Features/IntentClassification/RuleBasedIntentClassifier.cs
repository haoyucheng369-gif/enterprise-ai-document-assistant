namespace EnterpriseAiDocumentAssistant.Api.IntentClassification;

public sealed class RuleBasedIntentClassifier
{
    public IntentClassificationResult Classify(IntentClassificationRequest request)
    {
        // Deterministic rules keep local runs usable when model classification is unavailable.
        var message = request.Message.Trim();
        var intent = ClassifyMessage(message);

        return new IntentClassificationResult(
            intent,
            "Matched deterministic intent rules.",
            "rules");
    }

    private static string ClassifyMessage(string message)
    {
        // Rule order matters: explicit application actions win; normal document questions use RAG chat.
        if (ContainsAny(
            message,
            "run full workflow",
            "run document workflow",
            "complete document review",
            "执行完整流程",
            "运行完整流程",
            "完整审查文档"))
        {
            return "document_review_workflow";
        }

        if (ContainsAny(
            message,
            "review this resume",
            "analyze this resume",
            "resume review",
            "improve this cv",
            "评估这份简历",
            "分析简历优缺点",
            "优化这份简历",
            "简历评审"))
        {
            return "resume_review";
        }

        if (ContainsAny(
            message,
            "classify this document",
            "classify the document",
            "document classification",
            "determine the document category",
            "给这份文档分类",
            "文档分类",
            "判断文档类型"))
        {
            return "classification";
        }

        if (ContainsAny(
            message,
            "draft an email",
            "generate an email",
            "write a follow-up email",
            "create email draft",
            "生成邮件",
            "起草邮件",
            "写一封邮件",
            "生成后续邮件"))
        {
            return "email_draft";
        }

        if (ContainsAny(
            message,
            "analyze the risks",
            "analyze risks",
            "identify risks",
            "risk assessment",
            "complete risk analysis",
            "分析这份文档的风险",
            "分析风险",
            "识别风险",
            "风险评估"))
        {
            return "risk_analysis";
        }

        if (ContainsAny(
            message,
            "summarize this document",
            "summarize the document",
            "summarize the entire document",
            "full document summary",
            "generate a document summary",
            "总结这份文档",
            "总结整份文档",
            "总结全文",
            "概括全文",
            "生成文档摘要"))
        {
            return "summary";
        }

        if (ContainsAny(
            message,
            "check system health",
            "get document metadata",
            "document metadata",
            "execute health status tool",
            "检查系统健康状态",
            "获取文档元数据",
            "文档元数据"))
        {
            return "tool_request";
        }

        return "document_question";
    }

    private static bool ContainsAny(string value, params string[] keywords)
    {
        return keywords.Any(keyword => value.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
}
