namespace EnterpriseAiDocumentAssistant.Api.Skills;

public interface IResumeReviewSkill
{
    Task<ResumeReviewSkillResponse?> RunAsync(
        ResumeReviewSkillRequest request,
        CancellationToken cancellationToken);
}
