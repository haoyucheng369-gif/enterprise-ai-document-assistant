using System.Text.Json;
using EnterpriseAiDocumentAssistant.Api.AiGateway;
using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.Options;
using EnterpriseAiDocumentAssistant.Api.PromptOrchestration;
using EnterpriseAiDocumentAssistant.Api.Services;
using Microsoft.Extensions.Options;

namespace EnterpriseAiDocumentAssistant.Api.Skills;

public sealed class SummarySkill : ISummarySkill
{
    private readonly IAiGateway aiGateway;
    private readonly IApplicationDocumentProvider applicationDocumentProvider;
    private readonly AiGatewayOptions options;

    public SummarySkill(
        IApplicationDocumentProvider applicationDocumentProvider,
        IAiGateway aiGateway,
        IOptions<AiGatewayOptions> options)
    {
        this.applicationDocumentProvider = applicationDocumentProvider;
        this.aiGateway = aiGateway;
        this.options = options.Value;
    }

    public SummarySkillResponse? Run(SummarySkillRequest request)
    {
        // Synchronous execution is kept for harness/workflow tests that need deterministic local output.
        var document = applicationDocumentProvider.FindById(request.DocumentId);
        return document is null
            ? null
            : BuildDeterministicSummary(document);
    }

    public async Task<SummarySkillResponse?> RunAsync(
        SummarySkillRequest request,
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
            // Mock keeps the skill deterministic for local workflow and harness checks.
            return BuildDeterministicSummary(document);
        }

        var prompt = DocumentSkillPromptTemplates.BuildSummaryPrompt(document);
        var modelResponse = await aiGateway.GenerateChatResponseAsync(
            new ChatModelRequest(prompt, provider),
            cancellationToken);

        // Real model output is parsed into the same stable contract used by the Mock path.
        return TryParseModelSummary(document, modelResponse.Message.Answer)
            ?? BuildDeterministicSummary(document);
    }

    private static SummarySkillResponse BuildDeterministicSummary(DocumentItemResponse document)
    {
        // Fallback summary extracts short highlights from parsed sections when no real model is used.
        if (document.Sections.Count == 0)
        {
            return new SummarySkillResponse(
                document.Id,
                document.Title,
                $"{document.Title} is available, but no section content is ready for summarization.",
                [],
                []);
        }

        var contentHighlights = document.Sections
            .Select(section => ExtractHighlight(section.Body))
            .Where(highlight => !string.IsNullOrWhiteSpace(highlight))
            .Take(3)
            .ToArray();
        var keyPoints = document.Sections
            .Select(section => $"{section.Label}: {ExtractHighlight(section.Body)}")
            .ToArray();

        return new SummarySkillResponse(
            document.Id,
            document.Title,
            contentHighlights.Length == 0
                ? $"{document.Title} is available, but the extracted text is too limited for a useful summary."
                : $"{document.Title} includes {string.Join(" ", contentHighlights)}",
            keyPoints,
            document.Sections.Select(section => section.Label).ToArray());
    }

    private static string ExtractHighlight(string value)
    {
        var normalized = string.Join(
            " ",
            value.Split(
                [' ', '\t', '\r', '\n'],
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));

        const int maxLength = 180;
        return normalized.Length <= maxLength
            ? normalized
            : $"{normalized[..maxLength]}...";
    }

    private static SummarySkillResponse? TryParseModelSummary(
        DocumentItemResponse document,
        string answer)
    {
        // Real providers are expected to place minified JSON in the assistant answer field.
        if (!answer.TrimStart().StartsWith('{'))
        {
            return null;
        }

        try
        {
            using var jsonDocument = JsonDocument.Parse(answer);
            var root = jsonDocument.RootElement;

            return new SummarySkillResponse(
                document.Id,
                document.Title,
                SkillJsonReader.ReadString(root, "summary", $"{document.Title} was summarized by the selected model."),
                SkillJsonReader.ReadStringArray(root, "keyPoints"),
                SkillJsonReader.ReadStringArray(root, "sources"));
        }
        catch (JsonException)
        {
            return null;
        }
    }

}
