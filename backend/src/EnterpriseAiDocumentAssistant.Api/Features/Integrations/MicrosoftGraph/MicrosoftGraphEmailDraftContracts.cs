namespace EnterpriseAiDocumentAssistant.Api.Integrations.MicrosoftGraph;

public sealed record MicrosoftGraphEmailDraftRequest(
    string DocumentId,
    string To,
    string Subject,
    string Body);

public sealed record MicrosoftGraphEmailDraftResponse(
    string DraftId,
    string Provider,
    string Status,
    string DocumentId,
    string To,
    string Subject,
    string BodyPreview,
    string WebUrl,
    DateTimeOffset CreatedAtUtc);
