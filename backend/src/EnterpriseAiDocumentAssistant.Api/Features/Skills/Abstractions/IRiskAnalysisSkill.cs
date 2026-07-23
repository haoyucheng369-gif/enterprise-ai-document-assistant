namespace EnterpriseAiDocumentAssistant.Api.Skills;

public interface IRiskAnalysisSkill
{
    RiskAnalysisSkillResponse? Run(RiskAnalysisSkillRequest request);

    Task<RiskAnalysisSkillResponse?> RunAsync(
        RiskAnalysisSkillRequest request,
        CancellationToken cancellationToken);
}
