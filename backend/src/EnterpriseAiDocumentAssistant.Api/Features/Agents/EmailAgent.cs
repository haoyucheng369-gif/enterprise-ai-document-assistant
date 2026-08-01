using EnterpriseAiDocumentAssistant.Api.Skills;

namespace EnterpriseAiDocumentAssistant.Api.Agents;

public sealed class EmailAgent : IEmailAgent
{
    private readonly IEmailDraftSkill emailDraftSkill;

    public EmailAgent(IEmailDraftSkill emailDraftSkill)
    {
        this.emailDraftSkill = emailDraftSkill;
    }

    public EmailDraftSkillResponse? CreateDraft(DocumentAgentHandoff handoff)
    {
        // EmailAgent consumes the handoff instead of recomputing document analysis.
        return emailDraftSkill.Run(
            CreateRequest(handoff),
            handoff.Summary,
            handoff.RiskAnalysis);
    }

    public Task<EmailDraftSkillResponse?> CreateDraftAsync(
        DocumentAgentHandoff handoff,
        CancellationToken cancellationToken)
    {
        // Step 3.1: convert handoff metadata into the skill request and reuse its summary/risk objects.
        // EmailDraftSkill may call the model and metadata tool, but it does not repeat DocumentAgent work.
        return emailDraftSkill.RunAsync(
            CreateRequest(handoff),
            handoff.Summary,
            handoff.RiskAnalysis,
            cancellationToken);
    }

    private static EmailDraftSkillRequest CreateRequest(DocumentAgentHandoff handoff)
    {
        return new EmailDraftSkillRequest(
            handoff.DocumentId,
            handoff.EmailPurpose,
            handoff.AiProvider);
    }
}
