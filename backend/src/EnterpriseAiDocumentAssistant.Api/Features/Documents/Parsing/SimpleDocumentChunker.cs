namespace EnterpriseAiDocumentAssistant.Api.DocumentParsing;

public sealed class SimpleDocumentChunker : IDocumentChunker
{
    private const int MaxPreviewChunks = 6;
    private const int MaxChunkLength = 900;
    private const int MaxGeneratedTitleLength = 72;

    public IReadOnlyList<DocumentPreviewSection> BuildPreviewSections(
        string text,
        IReadOnlyList<string> warnings)
    {
        // Empty extraction still returns one preview section so the UI can show the failure reason.
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

        // These sections back preview, persistence, skills, and the current RAG index.
        for (var index = 0; index < normalizedText.Length && chunks.Count < MaxPreviewChunks; index += MaxChunkLength)
        {
            var length = Math.Min(MaxChunkLength, normalizedText.Length - index);
            var chunkBody = normalizedText.Substring(index, length).Trim();

            // Generate a readable title so preview entries and citations can identify the chunk.
            chunks.Add(new DocumentPreviewSection(
                $"Chunk {chunks.Count + 1}",
                BuildChunkTitle(chunkBody),
                chunkBody));
        }

        return chunks;
    }

    private static string NormalizeWhitespace(string text)
    {
        // Normalize paragraph spacing and repeated whitespace before creating readable preview chunks.
        return string.Join(
            Environment.NewLine + Environment.NewLine,
            text.Split(
                    [Environment.NewLine + Environment.NewLine],
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(block => string.Join(" ", block.Split(
                    [' ', '\t', '\r', '\n'],
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))));
    }

    private static string BuildChunkTitle(string chunkBody)
    {
        // Use the first sentence-like fragment instead of a fixed "Extracted text" label.
        var titleSource = chunkBody
            .Split(['.', '!', '?', '\r', '\n'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(titleSource))
        {
            return "Extracted content";
        }

        return titleSource.Length <= MaxGeneratedTitleLength
            ? titleSource
            : $"{titleSource[..MaxGeneratedTitleLength].TrimEnd()}...";
    }
}
