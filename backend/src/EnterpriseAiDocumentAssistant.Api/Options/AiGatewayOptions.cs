namespace EnterpriseAiDocumentAssistant.Api.Options;

public sealed class AiGatewayOptions
{
    public const string SectionName = "AiGateway";

    public string Provider { get; init; } = "Mock";

    public string ChatModel { get; init; } = "mock-document-assistant";
}
