using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.Documents;
using EnterpriseAiDocumentAssistant.Api.DocumentUpload;
using EnterpriseAiDocumentAssistant.Api.Options;
using Microsoft.Extensions.Options;

namespace EnterpriseAiDocumentAssistant.Api.Rag;

public sealed class RagService : IRagService
{
    private const int MaxChunkTextForPrompt = 1400;

    private readonly IDocumentRepository documentRepository;
    private readonly IEmbeddingGateway embeddingGateway;
    private readonly IVectorStore vectorStore;
    private readonly RagOptions options;

    public RagService(
        IDocumentRepository documentRepository,
        IEmbeddingGateway embeddingGateway,
        IVectorStore vectorStore,
        IOptions<RagOptions> options)
    {
        this.documentRepository = documentRepository;
        this.embeddingGateway = embeddingGateway;
        this.vectorStore = vectorStore;
        this.options = options.Value;
    }

    public Task IndexDocumentAsync(
        DocumentUploadResponse document,
        string? providerOverride,
        CancellationToken cancellationToken)
    {
        // Indexing starts from the persisted document shape used by the rest of the application.
        var sections = document.Sections
            .Select(section => new DocumentSectionResponse(section.Label, section.Title, section.Body))
            .ToArray();

        var documentItem = new DocumentItemResponse(
            document.Id,
            document.Title,
            document.Type,
            document.UpdatedAt,
            document.Status,
            sections);

        return IndexDocumentAsync(documentItem, providerOverride, cancellationToken);
    }

    public Task DeleteDocumentAsync(
        string documentId,
        CancellationToken cancellationToken)
    {
        return vectorStore.DeleteDocumentAsync(documentId, cancellationToken);
    }

    public async Task<RagRetrievalResult> RetrieveAsync(
        ChatRequest request,
        CancellationToken cancellationToken)
    {
        // Query flow: load document -> ensure index -> embed question -> search -> filter -> build prompt context.
        if (string.IsNullOrWhiteSpace(request.DocumentId))
        {
            return Empty(RagRetrievalStatus.NoDocumentSelected);
        }

        var uploadedDocument = documentRepository.FindById(request.DocumentId);
        var document = uploadedDocument is null ? null : ToDocumentItem(uploadedDocument);
        if (document is null)
        {
            return Empty(RagRetrievalStatus.DocumentNotFound);
        }

        var provider = ResolveProvider(request.AiProvider);
        await EnsureIndexedAsync(provider, document, cancellationToken);

        // The question vector is used only for retrieval; the chat model receives matched source text.
        var queryEmbedding = await embeddingGateway.GenerateEmbeddingAsync(
            new EmbeddingRequest(request.Message, provider),
            cancellationToken);

        var candidates = await vectorStore.SearchAsync(
            provider,
            document.Id,
            queryEmbedding.Vector,
            options.TopK,
            cancellationToken);

        // Only reliable matches become model context; weak results trigger the no-answer path.
        var matches = candidates
            .Where(match => match.Score >= options.MinimumSimilarityScore)
            .ToArray();

        if (matches.Length == 0)
        {
            return Empty(RagRetrievalStatus.InsufficientEvidence);
        }

        return new RagRetrievalResult(
            RagRetrievalStatus.Retrieved,
            BuildPromptContext(document, matches),
            BuildCitations(matches));
    }

    private static DocumentItemResponse ToDocumentItem(DocumentUploadResponse document)
    {
        return new DocumentItemResponse(
            document.Id,
            document.Title,
            document.Type,
            document.UpdatedAt,
            document.Status,
            document.Sections
                .Select(section => new DocumentSectionResponse(section.Label, section.Title, section.Body))
                .ToArray());
    }

    private async Task EnsureIndexedAsync(
        string provider,
        DocumentItemResponse document,
        CancellationToken cancellationToken)
    {
        if (await vectorStore.HasDocumentAsync(provider, document.Id, cancellationToken))
        {
            return;
        }

        // Rebuild derived vectors lazily from MongoDB when the in-memory index is empty.
        await IndexDocumentAsync(document, provider, cancellationToken);
    }

    private async Task IndexDocumentAsync(
        DocumentItemResponse document,
        string? providerOverride,
        CancellationToken cancellationToken)
    {
        var provider = ResolveProvider(providerOverride);
        var chunks = new List<VectorChunk>();

        // Each parsed section keeps its source text beside the generated vector.
        foreach (var section in document.Sections)
        {
            if (string.IsNullOrWhiteSpace(section.Body))
            {
                continue;
            }

            var embedding = await embeddingGateway.GenerateEmbeddingAsync(
                new EmbeddingRequest($"{section.Title}\n{section.Body}", provider),
                cancellationToken);

            chunks.Add(new VectorChunk(
                provider,
                document.Id,
                document.Title,
                $"{document.Id}:{section.Label}",
                section.Label,
                section.Title,
                section.Body,
                embedding.Vector));
        }

        await vectorStore.UpsertDocumentChunksAsync(provider, document.Id, chunks, cancellationToken);
    }

    private static RagRetrievalResult Empty(RagRetrievalStatus status)
    {
        return new RagRetrievalResult(status, string.Empty, []);
    }

    private static string BuildPromptContext(
        DocumentItemResponse document,
        IReadOnlyList<VectorSearchResult> matches)
    {
        // Convert matched source text, not vectors, into the context sent to the chat model.
        var chunks = matches.Select((match, index) =>
            $"""
            Retrieved chunk {index + 1}
            Source: {match.Chunk.Label} - {match.Chunk.Title}
            Score: {match.Score:0.000}
            Text: {Truncate(match.Chunk.Text, MaxChunkTextForPrompt)}
            """);

        return $"""
            Selected document id: {document.Id}
            Title: {document.Title}
            Type: {document.Type}
            Retrieval mode: in-memory vector search
            Retrieved evidence:
            {string.Join(Environment.NewLine, chunks)}
            """;
    }

    private static IReadOnlyList<string> BuildCitations(IReadOnlyList<VectorSearchResult> matches)
    {
        return matches
            .Select(match => $"{match.Chunk.Label} - {match.Chunk.Title}: {Truncate(match.Chunk.Text, 120)}")
            .ToArray();
    }

    private static string ResolveProvider(string? provider)
    {
        return string.IsNullOrWhiteSpace(provider)
            ? "Mock"
            : provider.Trim();
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength
            ? value
            : $"{value[..maxLength]}...";
    }
}
