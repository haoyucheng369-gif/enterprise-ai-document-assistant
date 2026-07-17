namespace EnterpriseAiDocumentAssistant.Api.Skills;

public sealed class EmailDraftSkill : IEmailDraftSkill
{
    private readonly ISummarySkill summarySkill;
    private readonly IRiskAnalysisSkill riskAnalysisSkill;

    public EmailDraftSkill(
        ISummarySkill summarySkill,
        IRiskAnalysisSkill riskAnalysisSkill)
    {
        this.summarySkill = summarySkill;
        this.riskAnalysisSkill = riskAnalysisSkill;
    }

    public EmailDraftSkillResponse? Run(EmailDraftSkillRequest request)
    {
        var summary = summarySkill.Run(new SummarySkillRequest(request.DocumentId));
        var riskAnalysis = riskAnalysisSkill.Run(new RiskAnalysisSkillRequest(request.DocumentId));

        if (summary is null || riskAnalysis is null)
        {
            return null;
        }

        var topRisks = riskAnalysis.Risks.Take(3).ToArray();
        var riskLines = topRisks.Length == 0
            ? "No specific risk items were identified from the available sections."
            : string.Join(Environment.NewLine, topRisks.Select(risk =>
                $"- {risk.Title} ({risk.Source}, {risk.Severity}): {risk.Recommendation}"));

        var purpose = string.IsNullOrWhiteSpace(request.Purpose)
            ? "Clarify the highlighted contract items."
            : request.Purpose.Trim();

        var body = $"""
        Hi,

        I reviewed {summary.Title} for the following purpose: {purpose}

        Summary:
        {summary.Summary}

        Items to clarify:
        {riskLines}

        Please confirm the business position for these items and share any proposed updates.

        Thanks,
        """;

        return new EmailDraftSkillResponse(
            summary.DocumentId,
            $"Questions about {summary.Title}",
            body,
            summary.Sources
                .Concat(topRisks.Select(risk => risk.Source))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            topRisks.Select(risk => risk.Recommendation).ToArray());
    }
}
