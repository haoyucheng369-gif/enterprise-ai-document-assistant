namespace EnterpriseAiDocumentAssistant.Api.Agents;

public interface IDocumentAgent
{
    DocumentAgentHandoff? PrepareHandoff(DocumentAgentRequest request);

    Task<DocumentAgentHandoff?> PrepareHandoffAsync(
        DocumentAgentRequest request,
        CancellationToken cancellationToken);
}
