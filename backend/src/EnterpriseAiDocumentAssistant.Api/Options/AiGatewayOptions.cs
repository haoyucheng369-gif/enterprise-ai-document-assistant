namespace EnterpriseAiDocumentAssistant.Api.Options;

public sealed class AiGatewayOptions
{
    public const string SectionName = "AiGateway";

    public string Provider { get; init; } = "Mock";

    public string ChatModel { get; init; } = "mock-document-assistant";

    public string ApiKey { get; init; } = string.Empty;

    public string Endpoint { get; init; } = "https://api.openai.com";

    public string ApiVersion { get; init; } = "2024-10-21";

    public int TimeoutSeconds { get; init; } = 30;
}
