namespace EnterpriseAiDocumentAssistant.Api.Options;

public sealed class AiGatewayOptions
{
    public const string SectionName = "AiGateway";

    public string Provider { get; init; } = "None";
}
