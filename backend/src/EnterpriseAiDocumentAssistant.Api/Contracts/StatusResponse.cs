namespace EnterpriseAiDocumentAssistant.Api.Contracts;

public sealed record StatusResponse(
    string Service,
    string Environment,
    string ApiVersion,
    string Version,
    string AiProvider,
    DateTimeOffset TimeUtc);
