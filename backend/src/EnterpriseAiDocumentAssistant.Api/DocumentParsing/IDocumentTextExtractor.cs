namespace EnterpriseAiDocumentAssistant.Api.DocumentParsing;

public interface IDocumentTextExtractor
{
    Task<DocumentTextExtractionResult> ExtractAsync(
        IFormFile file,
        string extension,
        CancellationToken cancellationToken);
}
