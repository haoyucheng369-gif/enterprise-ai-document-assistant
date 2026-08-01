using EnterpriseAiDocumentAssistant.Api.Skills;

namespace EnterpriseAiDocumentAssistant.Api.Agents;

public sealed class DocumentAgent : IDocumentAgent
{
    private readonly ISummarySkill summarySkill;
    private readonly IRiskAnalysisSkill riskAnalysisSkill;

    public DocumentAgent(
        ISummarySkill summarySkill,
        IRiskAnalysisSkill riskAnalysisSkill)
    {
        this.summarySkill = summarySkill;
        this.riskAnalysisSkill = riskAnalysisSkill;
    }

    public DocumentAgentHandoff? PrepareHandoff(DocumentAgentRequest request)
    {
        // DocumentAgent owns document understanding and hands typed results to the next agent.
        var summary = summarySkill.Run(new SummarySkillRequest(request.DocumentId));
        if (summary is null)
        {
            return null;
        }

        var riskAnalysis = riskAnalysisSkill.Run(new RiskAnalysisSkillRequest(request.DocumentId));

        return CreateHandoff(request, summary, riskAnalysis);
    }

    public async Task<DocumentAgentHandoff?> PrepareHandoffAsync(
        DocumentAgentRequest request,
        CancellationToken cancellationToken)
    {
        // Step 2.1: summarize the selected document with the requested AI provider.
        var summary = await summarySkill.RunAsync(
            new SummarySkillRequest(request.DocumentId, request.AiProvider),
            cancellationToken);
        if (summary is null)
        {
            return null;
        }

        // Step 2.2: analyze risks only after a valid summary has been produced.
        var riskAnalysis = await riskAnalysisSkill.RunAsync(
            new RiskAnalysisSkillRequest(request.DocumentId, request.AiProvider),
            cancellationToken);

        // Step 2.3: package both typed skill results for EmailAgent; no text parsing is needed downstream.
        return CreateHandoff(request, summary, riskAnalysis);
    }

    private static DocumentAgentHandoff? CreateHandoff(
        DocumentAgentRequest request,
        SummarySkillResponse? summary,
        RiskAnalysisSkillResponse? riskAnalysis)
    {
        // A missing skill result stops the handoff so the workflow cannot continue with incomplete data.
        if (summary is null || riskAnalysis is null)
        {
            return null;
        }

        return new DocumentAgentHandoff(
            $"handoff-{Guid.NewGuid():N}",
            request.DocumentId,
            request.EmailPurpose,
            request.AiProvider,
            summary,
            riskAnalysis);
    }
}
