using EnterpriseAiDocumentAssistant.Api.Contracts;

namespace EnterpriseAiDocumentAssistant.Api.PromptOrchestration;

public static class DocumentSkillPromptTemplates
{
    private const int DocumentTextLimit = 16000;

    public static OrchestratedPrompt BuildSummaryPrompt(DocumentItemResponse document)
    {
        var documentText = BuildLimitedDocumentTextForPrompt(document);
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
        var documentText = BuildLimitedDocumentTextForPrompt(document);
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
        var documentText = BuildLimitedDocumentTextForPrompt(document);
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
        IReadOnlyList<string> risks,
        string metadata)
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
            new PromptVariable("risks", riskText),
            new PromptVariable("metadata", metadata)
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

            Metadata from tool:
            {metadata}
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

    public static OrchestratedPrompt BuildResumeReviewPrompt(
        DocumentItemResponse document,
        string instruction)
    {
        var documentText = BuildLimitedDocumentTextForPrompt(document);
        var normalizedInstruction = string.IsNullOrWhiteSpace(instruction)
            ? "Create a practical resume review brief."
            : instruction.Trim();
        var variables = new[]
        {
            new PromptVariable("document_title", document.Title),
            new PromptVariable("document_type", document.Type),
            new PromptVariable("instruction", normalizedInstruction),
            new PromptVariable("document_text", documentText)
        };

        var userMessage = $"""
            Create a resume review brief from this resume or professional profile.

            Title: {document.Title}
            Type hint: {document.Type}
            Instruction: {normalizedInstruction}

            Source document:
            {documentText}
            """;

        // The draft is returned as Markdown so the UI can render it without generating a binary file yet.
        return new OrchestratedPrompt(
            "resume-review-v1",
            EnterpriseAssistantPromptDefaults.BuildSystemMessage(
                "Review resumes and professional profiles and produce practical Markdown review briefs."),
            userMessage,
            EnterpriseAssistantPromptDefaults.CombineOutputRules(
                EnterpriseAssistantPromptDefaults.OutputRules,
                [
                    "Set answer to a single minified JSON object with title, format, content, basedOn, and nextActions.",
                    "format must be markdown.",
                    "content must be a complete Markdown resume review brief.",
                    "Write title, content, and nextActions in Chinese. Keep common technical terms such as .NET, React, Azure, SQL, CI/CD, and API in English when clearer.",
                    "content must include Strengths, Weaknesses, Missing Keywords, Suggested Positioning, and Rewrite Instructions for ChatGPT sections.",
                    "Preserve truthful information from the source document and do not invent employers, dates, diplomas, or certifications.",
                    "basedOn must be an array of short source strings.",
                    "nextActions must be an array of short user-facing action strings."
                ]),
            variables);
    }

    private static string BuildLimitedDocumentTextForPrompt(DocumentItemResponse document)
    {
        // Skills use parsed document sections as model context, but keep a hard limit to avoid oversized prompts.
        var documentText = string.Join(
            Environment.NewLine,
            document.Sections.Select(section =>
                $"{section.Label} - {section.Title}: {section.Body}"));

        if (string.IsNullOrWhiteSpace(documentText))
        {
            documentText = $"{document.Type} document titled {document.Title}.";
        }

        return Truncate(documentText, DocumentTextLimit);
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength
            ? value
            : value[..maxLength];
    }
}
