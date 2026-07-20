namespace EnterpriseAiDocumentAssistant.Api.Skills;

public interface ISummarySkill
{
    SummarySkillResponse? Run(SummarySkillRequest request);
}
