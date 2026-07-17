using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.Services;

namespace EnterpriseAiDocumentAssistant.Api.Skills;

public sealed class RiskAnalysisSkill : IRiskAnalysisSkill
{
    private readonly IWorkspaceDataProvider workspaceDataProvider;

    public RiskAnalysisSkill(IWorkspaceDataProvider workspaceDataProvider)
    {
        this.workspaceDataProvider = workspaceDataProvider;
    }

    public RiskAnalysisSkillResponse? Run(RiskAnalysisSkillRequest request)
    {
        var document = workspaceDataProvider.GetWorkspace()
            .Documents
            .FirstOrDefault(candidate =>
                string.Equals(candidate.Id, request.DocumentId, StringComparison.OrdinalIgnoreCase));

        if (document is null)
        {
            return null;
        }

        if (document.Sections.Count == 0)
        {
            return new RiskAnalysisSkillResponse(
                document.Id,
                document.Title,
                [],
                ["Document sections are not available for risk analysis."]);
        }

        var risks = document.Sections
            .SelectMany(AnalyzeSection)
            .ToArray();

        return new RiskAnalysisSkillResponse(
            document.Id,
            document.Title,
            risks,
            risks.Length == 0 ? ["No obvious risk signals were found in the available sections."] : []);
    }

    private static IEnumerable<RiskItem> AnalyzeSection(DocumentSectionResponse section)
    {
        var text = $"{section.Title} {section.Body}";

        // First version uses deterministic signals; AI Gateway can replace this with model reasoning later.
        if (ContainsAny(text, ["renew", "notice"]))
        {
            yield return new RiskItem(
                "Automatic renewal notice window",
                "medium",
                section.Label,
                "Confirm the business owner and add a reminder before the notice deadline.");
        }

        if (ContainsAny(text, ["liability", "cap", "confidentiality", "data protection"]))
        {
            yield return new RiskItem(
                "Liability cap and exclusions",
                "high",
                section.Label,
                "Review whether confidentiality and data protection remedies are acceptable.");
        }

        if (ContainsAny(text, ["service credit", "availability", "fifteen days"]))
        {
            yield return new RiskItem(
                "Service credit claim deadline",
                "medium",
                section.Label,
                "Confirm the operations team can request credits within the required window.");
        }
    }

    private static bool ContainsAny(string value, IReadOnlyList<string> signals)
    {
        return signals.Any(signal =>
            value.Contains(signal, StringComparison.OrdinalIgnoreCase));
    }
}
