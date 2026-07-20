using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UglyToad.PdfPig;

namespace EnterpriseAiDocumentAssistant.Api.DocumentParsing;

public sealed class DocumentTextExtractor : IDocumentTextExtractor
{
    public async Task<DocumentTextExtractionResult> ExtractAsync(
        IFormFile file,
        string extension,
        CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);
        stream.Position = 0;

        return extension.ToLowerInvariant() switch
        {
            ".txt" or ".md" => await ExtractPlainTextAsync(stream, cancellationToken),
            ".pdf" => ExtractPdfText(stream),
            ".docx" => ExtractDocxText(stream),
            _ => new DocumentTextExtractionResult(string.Empty, ["Unsupported file type."])
        };
    }

    private static async Task<DocumentTextExtractionResult> ExtractPlainTextAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            leaveOpen: true);

        var text = await reader.ReadToEndAsync(cancellationToken);
        return new DocumentTextExtractionResult(text, []);
    }

    private static DocumentTextExtractionResult ExtractPdfText(Stream stream)
    {
        var warnings = new List<string>();
        var builder = new StringBuilder();

        using var document = PdfDocument.Open(stream);
        foreach (var page in document.GetPages())
        {
            var pageText = page.Text;
            if (string.IsNullOrWhiteSpace(pageText))
            {
                warnings.Add($"Page {page.Number} did not contain extractable text.");
                continue;
            }

            builder
                .AppendLine($"Page {page.Number}")
                .AppendLine(pageText)
                .AppendLine();
        }

        return new DocumentTextExtractionResult(builder.ToString(), warnings);
    }

    private static DocumentTextExtractionResult ExtractDocxText(Stream stream)
    {
        using var document = WordprocessingDocument.Open(stream, false);
        var body = document.MainDocumentPart?.Document?.Body;
        if (body is null)
        {
            return new DocumentTextExtractionResult(string.Empty, ["DOCX body was empty."]);
        }

        // Paragraph-level extraction gives the preview readable structure without preserving full formatting.
        var paragraphs = body
            .Descendants<Paragraph>()
            .Select(paragraph => string.Concat(paragraph.Descendants<Text>().Select(text => text.Text)).Trim())
            .Where(text => !string.IsNullOrWhiteSpace(text));

        return new DocumentTextExtractionResult(string.Join(Environment.NewLine + Environment.NewLine, paragraphs), []);
    }
}
