namespace EnterpriseAiDocumentAssistant.Api.DocumentParsing;

public sealed record DocumentTextExtractionResult(
    string Text,
    IReadOnlyList<string> Warnings);
