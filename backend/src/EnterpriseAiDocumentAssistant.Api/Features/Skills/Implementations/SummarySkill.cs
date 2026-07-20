using EnterpriseAiDocumentAssistant.Api.Services;

namespace EnterpriseAiDocumentAssistant.Api.Skills;

public sealed class SummarySkill : ISummarySkill
{
    private readonly IApplicationDocumentProvider applicationDocumentProvider;

    public SummarySkill(IApplicationDocumentProvider applicationDocumentProvider)
    {
        this.applicationDocumentProvider = applicationDocumentProvider;
    }

    public SummarySkillResponse? Run(SummarySkillRequest request)
    {
        var document = applicationDocumentProvider.FindById(request.DocumentId);
        if (document is null)
        {
            return null;
        }

        if (document.Sections.Count == 0)
        {
            return new SummarySkillResponse(
                document.Id,
                document.Title,
                $"{document.Title} is available, but no section content is ready for summarization.",
                [],
                []);
        }

        var sectionTitles = document.Sections.Select(section => section.Title).ToArray();
        var keyPoints = document.Sections
            .Select(section => $"{section.Label}: {section.Title}. {section.Body}")
            .ToArray();

        return new SummarySkillResponse(
            document.Id,
            document.Title,
            $"{document.Title} focuses on {string.Join(", ", sectionTitles)}.",
            keyPoints,
            document.Sections.Select(section => section.Label).ToArray());
    }
}
