namespace EnterpriseAiDocumentAssistant.Api.Skills;

public interface IClassificationSkill
{
    Task<ClassificationSkillResponse?> RunAsync(
        ClassificationSkillRequest request,
        CancellationToken cancellationToken);
}
