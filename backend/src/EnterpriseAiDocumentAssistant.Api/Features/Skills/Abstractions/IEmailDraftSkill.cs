namespace EnterpriseAiDocumentAssistant.Api.Skills;

public interface IEmailDraftSkill
{
    EmailDraftSkillResponse? Run(EmailDraftSkillRequest request);

    EmailDraftSkillResponse? Run(
        EmailDraftSkillRequest request,
        SummarySkillResponse summary,
        RiskAnalysisSkillResponse riskAnalysis);

    Task<EmailDraftSkillResponse?> RunAsync(
        EmailDraftSkillRequest request,
        CancellationToken cancellationToken);

    Task<EmailDraftSkillResponse?> RunAsync(
        EmailDraftSkillRequest request,
        SummarySkillResponse summary,
        RiskAnalysisSkillResponse riskAnalysis,
        CancellationToken cancellationToken);
}
