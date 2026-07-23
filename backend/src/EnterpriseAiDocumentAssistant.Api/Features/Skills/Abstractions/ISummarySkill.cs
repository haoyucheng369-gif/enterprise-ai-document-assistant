namespace EnterpriseAiDocumentAssistant.Api.Skills;

public interface ISummarySkill
{
    SummarySkillResponse? Run(SummarySkillRequest request);

    Task<SummarySkillResponse?> RunAsync(
        SummarySkillRequest request,
        CancellationToken cancellationToken);
}
