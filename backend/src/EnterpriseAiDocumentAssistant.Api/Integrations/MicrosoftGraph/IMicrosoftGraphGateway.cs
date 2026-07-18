namespace EnterpriseAiDocumentAssistant.Api.Integrations.MicrosoftGraph;

public interface IMicrosoftGraphGateway
{
    MicrosoftGraphEmailDraftResponse CreateEmailDraft(MicrosoftGraphEmailDraftRequest request);
}
