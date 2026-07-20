namespace EnterpriseAiDocumentAssistant.Api.DocumentParsing;

public interface IDocumentChunker
{
    IReadOnlyList<DocumentPreviewSection> BuildPreviewSections(string text, IReadOnlyList<string> warnings);
}
