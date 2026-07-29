namespace EnterpriseAiDocumentAssistant.Api.Options;

public sealed class RagOptions
{
    public const string SectionName = "Rag";

    public int TopK { get; init; } = 4;

    // Results below this score are not trusted as evidence for a grounded answer.
    public double MinimumSimilarityScore { get; init; } = 0.30;
}
