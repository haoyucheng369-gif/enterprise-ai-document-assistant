namespace EnterpriseAiDocumentAssistant.Api.Options;

public sealed class QdrantOptions
{
    public const string SectionName = "Qdrant";

    public string Host { get; init; } = "localhost";

    public int GrpcPort { get; init; } = 6334;

    public string CollectionPrefix { get; init; } = "document_chunks";
}
