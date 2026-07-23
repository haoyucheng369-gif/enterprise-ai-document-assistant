using System.Text.Json;
using EnterpriseAiDocumentAssistant.Api.AiGateway;
using EnterpriseAiDocumentAssistant.Api.Options;
using EnterpriseAiDocumentAssistant.Api.PromptOrchestration;
using EnterpriseAiDocumentAssistant.Api.Services;
using Microsoft.Extensions.Options;

namespace EnterpriseAiDocumentAssistant.Api.Skills;

public sealed class EmailDraftSkill : IEmailDraftSkill
{
    private readonly IAiGateway aiGateway;
    private readonly IApplicationDocumentProvider applicationDocumentProvider;
    private readonly AiGatewayOptions options;
    private readonly ISummarySkill summarySkill;
    private readonly IRiskAnalysisSkill riskAnalysisSkill;

    public EmailDraftSkill(
        IApplicationDocumentProvider applicationDocumentProvider,
        IAiGateway aiGateway,
        IOptions<AiGatewayOptions> options,
        ISummarySkill summarySkill,
        IRiskAnalysisSkill riskAnalysisSkill)
    {
        this.applicationDocumentProvider = applicationDocumentProvider;
        this.aiGateway = aiGateway;
        this.options = options.Value;
        this.summarySkill = summarySkill;
        this.riskAnalysisSkill = riskAnalysisSkill;
    }

    public EmailDraftSkillResponse? Run(EmailDraftSkillRequest request)
    {
        // The local path composes deterministic summary and risk skills without making model calls.
        var summary = summarySkill.Run(new SummarySkillRequest(request.DocumentId));
        var riskAnalysis = riskAnalysisSkill.Run(new RiskAnalysisSkillRequest(request.DocumentId));

        if (summary is null || riskAnalysis is null)
        {
            return null;
        }

        return BuildDeterministicDraft(request, summary, riskAnalysis);
    }

    public async Task<EmailDraftSkillResponse?> RunAsync(
        EmailDraftSkillRequest request,
        CancellationToken cancellationToken)
    {
        var document = applicationDocumentProvider.FindById(request.DocumentId);
        if (document is null)
        {
            return null;
        }

        var provider = SkillJsonReader.ResolveProvider(options, request.AiProvider);
        var summary = await summarySkill.RunAsync(
            new SummarySkillRequest(request.DocumentId, provider),
            cancellationToken);
        var riskAnalysis = await riskAnalysisSkill.RunAsync(
            new RiskAnalysisSkillRequest(request.DocumentId, provider),
            cancellationToken);

        if (summary is null || riskAnalysis is null)
        {
            return null;
        }

        return await RunAsync(request, summary, riskAnalysis, cancellationToken);
    }

    public async Task<EmailDraftSkillResponse?> RunAsync(
        EmailDraftSkillRequest request,
        SummarySkillResponse summary,
        RiskAnalysisSkillResponse riskAnalysis,
        CancellationToken cancellationToken)
    {
        var document = applicationDocumentProvider.FindById(request.DocumentId);
        if (document is null)
        {
            return null;
        }

        var provider = SkillJsonReader.ResolveProvider(options, request.AiProvider);
        if (SkillJsonReader.IsMockProvider(provider))
        {
            // Mock uses composed deterministic skill results for repeatable local testing.
            return BuildDeterministicDraft(request, summary, riskAnalysis);
        }

        var purpose = NormalizePurpose(request.Purpose);
        var riskLines = riskAnalysis.Risks
            .Take(5)
            .Select(risk => $"{risk.Title} ({risk.Source}, {risk.Severity}): {risk.Recommendation}")
            .ToArray();
        var prompt = DocumentSkillPromptTemplates.BuildEmailDraftPrompt(
            document,
            purpose,
            summary.Summary,
            riskLines);
        var modelResponse = await aiGateway.GenerateChatResponseAsync(
            new ChatModelRequest(prompt, provider),
            cancellationToken);

        // Email output remains structured so UI/workflow code can render subject, body, and actions separately.
        return TryParseModelEmailDraft(document.Id, modelResponse.Message.Answer)
            ?? BuildDeterministicDraft(request, summary, riskAnalysis);
    }

    private static EmailDraftSkillResponse BuildDeterministicDraft(
        EmailDraftSkillRequest request,
        SummarySkillResponse summary,
        RiskAnalysisSkillResponse riskAnalysis)
    {
        var topRisks = riskAnalysis.Risks.Take(3).ToArray();
        var riskLines = topRisks.Length == 0
            ? "No specific risk items were identified from the available sections."
            : string.Join(Environment.NewLine, topRisks.Select(risk =>
                $"- {risk.Title} ({risk.Source}, {risk.Severity}): {risk.Recommendation}"));

        var purpose = NormalizePurpose(request.Purpose);

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

    private static EmailDraftSkillResponse? TryParseModelEmailDraft(
        string documentId,
        string answer)
    {
        if (!answer.TrimStart().StartsWith('{'))
        {
            return null;
        }

        try
        {
            using var jsonDocument = JsonDocument.Parse(answer);
            var root = jsonDocument.RootElement;

            return new EmailDraftSkillResponse(
                documentId,
                SkillJsonReader.ReadString(root, "subject", "Document follow-up"),
                SkillJsonReader.ReadString(root, "body", "Hi,\n\nPlease review the attached document items.\n\nThanks,"),
                SkillJsonReader.ReadStringArray(root, "basedOn"),
                SkillJsonReader.ReadStringArray(root, "nextActions"));
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string NormalizePurpose(string purpose)
    {
        return string.IsNullOrWhiteSpace(purpose)
            ? "Clarify the highlighted contract items."
            : purpose.Trim();
    }

}
