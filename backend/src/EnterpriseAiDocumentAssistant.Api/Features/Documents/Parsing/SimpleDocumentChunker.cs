namespace EnterpriseAiDocumentAssistant.Api.DocumentParsing;

public sealed class SimpleDocumentChunker : IDocumentChunker
{
    private const int MaxPreviewChunks = 6;
    private const int MaxChunkLength = 900;

    public IReadOnlyList<DocumentPreviewSection> BuildPreviewSections(
        string text,
        IReadOnlyList<string> warnings)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            var warningText = warnings.Count == 0
                ? "No extractable text was found."
                : string.Join(" ", warnings);

            return
            [
                new DocumentPreviewSection(
                    "Preview",
                    "No extractable text",
                    warningText)
            ];
        }

        var normalizedText = NormalizeWhitespace(text);
        var chunks = new List<DocumentPreviewSection>();

        for (var index = 0; index < normalizedText.Length && chunks.Count < MaxPreviewChunks; index += MaxChunkLength)
        {
            var length = Math.Min(MaxChunkLength, normalizedText.Length - index);
            chunks.Add(new DocumentPreviewSection(
                $"Chunk {chunks.Count + 1}",
                "Extracted text",
                normalizedText.Substring(index, length).Trim()));
        }

        return chunks;
    }

    private static string NormalizeWhitespace(string text)
    {
        return string.Join(
            Environment.NewLine + Environment.NewLine,
            text.Split(
                    [Environment.NewLine + Environment.NewLine],
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(block => string.Join(" ", block.Split(
                    [' ', '\t', '\r', '\n'],
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))));
    }
}
