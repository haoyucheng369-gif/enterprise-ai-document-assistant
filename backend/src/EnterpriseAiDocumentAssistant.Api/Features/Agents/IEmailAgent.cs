using EnterpriseAiDocumentAssistant.Api.Skills;

namespace EnterpriseAiDocumentAssistant.Api.Agents;

public interface IEmailAgent
{
    EmailDraftSkillResponse? CreateDraft(DocumentAgentHandoff handoff);

    Task<EmailDraftSkillResponse?> CreateDraftAsync(
        DocumentAgentHandoff handoff,
        CancellationToken cancellationToken);
}
