namespace EnterpriseAiDocumentAssistant.Api.Skills;

public interface IEmailDraftSkill
{
    EmailDraftSkillResponse? Run(EmailDraftSkillRequest request);
}
