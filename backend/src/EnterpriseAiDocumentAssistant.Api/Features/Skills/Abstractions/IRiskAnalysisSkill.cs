namespace EnterpriseAiDocumentAssistant.Api.Skills;

public interface IRiskAnalysisSkill
{
    RiskAnalysisSkillResponse? Run(RiskAnalysisSkillRequest request);
}
