using EnterpriseAiDocumentAssistant.Api.Contracts;

namespace EnterpriseAiDocumentAssistant.Api.PromptOrchestration;

public static class DocumentSkillPromptTemplates
{
    public static OrchestratedPrompt BuildSummaryPrompt(DocumentItemResponse document)
    {
        var documentText = BuildDocumentText(document);
        var variables = new[]
        {
            new PromptVariable("document_title", document.Title),
            new PromptVariable("document_type", document.Type),
            new PromptVariable("document_text", documentText)
        };

        var userMessage = $"""
            Summarize this document for a business user.

            Title: {document.Title}
            Type hint: {document.Type}

            Document text:
            {documentText}
            """;

        // The summary skill keeps model output parseable by putting a compact JSON object in answer.
        return new OrchestratedPrompt(
            "document-summary-v1",
            EnterpriseAssistantPromptDefaults.BuildSystemMessage(
                "Summarize enterprise documents with concise, source-grounded business language."),
            userMessage,
            EnterpriseAssistantPromptDefaults.CombineOutputRules(
                EnterpriseAssistantPromptDefaults.OutputRules,
                [
                    "Set answer to a single minified JSON object with summary, keyPoints, and sources.",
                    "summary must be one concise paragraph.",
                    "keyPoints must be an array of 3 to 6 short strings.",
                    "sources must be an array of section labels or document references from the provided context."
                ]),
            variables);
    }

    public static OrchestratedPrompt BuildClassificationPrompt(DocumentItemResponse document)
    {
        var documentText = BuildDocumentText(document);
        var variables = new[]
        {
            new PromptVariable("document_title", document.Title),
            new PromptVariable("document_type", document.Type),
            new PromptVariable("document_text", documentText)
        };

        var userMessage = $"""
            Classify this document.

            Title: {document.Title}
            Type hint: {document.Type}

            Document text:
            {documentText}
            """;

        // Classification asks for compact JSON so the API can validate and map it into a stable contract.
        return new OrchestratedPrompt(
            "document-classification-v1",
            EnterpriseAssistantPromptDefaults.BuildSystemMessage(
                "Classify enterprise documents for a document assistant. Keep the classification practical and conservative."),
            userMessage,
            EnterpriseAssistantPromptDefaults.CombineOutputRules(
                EnterpriseAssistantPromptDefaults.OutputRules,
                [
                    "Set answer to a single minified JSON object with category, priority, confidence, reason, signals, and sources. Do not return only the category label.",
                    "Allowed category values: Contract, Policy, Report, Resume, TechnicalDocument, Other.",
                    "Allowed priority values: Low, Medium, High.",
                    "Confidence must be a number from 0 to 1.",
                    "Signals and sources must be arrays of short strings."
                ]),
            variables);
    }

    public static OrchestratedPrompt BuildRiskAnalysisPrompt(DocumentItemResponse document)
    {
        var documentText = BuildDocumentText(document);
        var variables = new[]
        {
            new PromptVariable("document_title", document.Title),
            new PromptVariable("document_type", document.Type),
            new PromptVariable("document_text", documentText)
        };

        var userMessage = $"""
            Review this document for practical business risks.

            Title: {document.Title}
            Type hint: {document.Type}

            Document text:
            {documentText}
            """;

        // Risk analysis returns structured items so workflow and UI code can display severity and source.
        return new OrchestratedPrompt(
            "document-risk-analysis-v1",
            EnterpriseAssistantPromptDefaults.BuildSystemMessage(
                "Identify practical enterprise document risks using only the provided document context."),
            userMessage,
            EnterpriseAssistantPromptDefaults.CombineOutputRules(
                EnterpriseAssistantPromptDefaults.OutputRules,
                [
                    "Set answer to a single minified JSON object with risks and missingInformation.",
                    "risks must be an array of objects with title, severity, source, and recommendation.",
                    "Allowed severity values: low, medium, high.",
                    "missingInformation must be an array of short strings.",
                    "Return an empty risks array when no grounded risk is visible."
                ]),
            variables);
    }

    public static OrchestratedPrompt BuildEmailDraftPrompt(
        DocumentItemResponse document,
        string purpose,
        string summary,
        IReadOnlyList<string> risks)
    {
        var riskText = risks.Count == 0
            ? "No specific risks were identified."
            : string.Join(Environment.NewLine, risks.Select(risk => $"- {risk}"));
        var variables = new[]
        {
            new PromptVariable("document_title", document.Title),
            new PromptVariable("document_type", document.Type),
            new PromptVariable("purpose", purpose),
            new PromptVariable("summary", summary),
            new PromptVariable("risks", riskText)
        };

        var userMessage = $"""
            Prepare a concise follow-up email draft.

            Title: {document.Title}
            Type hint: {document.Type}
            Purpose: {purpose}

            Summary:
            {summary}

            Risk items:
            {riskText}
            """;

        // Email draft output stays structured so the UI can show subject, body, sources, and next actions separately.
        return new OrchestratedPrompt(
            "document-email-draft-v1",
            EnterpriseAssistantPromptDefaults.BuildSystemMessage(
                "Draft practical business follow-up emails from document review results."),
            userMessage,
            EnterpriseAssistantPromptDefaults.CombineOutputRules(
                EnterpriseAssistantPromptDefaults.OutputRules,
                [
                    "Set answer to a single minified JSON object with subject, body, basedOn, and nextActions.",
                    "subject must be a short email subject.",
                    "body must be a ready-to-send email body.",
                    "basedOn must be an array of short source strings.",
                    "nextActions must be an array of short user-facing action strings."
                ]),
            variables);
    }

    private static string BuildDocumentText(DocumentItemResponse document)
    {
        var documentText = string.Join(
            Environment.NewLine,
            document.Sections.Select(section =>
                $"{section.Label} - {section.Title}: {section.Body}"));

        if (string.IsNullOrWhiteSpace(documentText))
        {
            documentText = $"{document.Type} document titled {document.Title}.";
        }

        return Truncate(documentText, 6000);
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength
            ? value
            : value[..maxLength];
    }
}
