using System.Text.Json;
using EnterpriseAiDocumentAssistant.Api.AiGateway;
using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.Options;
using EnterpriseAiDocumentAssistant.Api.PromptOrchestration;
using EnterpriseAiDocumentAssistant.Api.Services;
using Microsoft.Extensions.Options;

namespace EnterpriseAiDocumentAssistant.Api.Skills;

public sealed class RiskAnalysisSkill : IRiskAnalysisSkill
{
    private readonly IAiGateway aiGateway;
    private readonly IApplicationDocumentProvider applicationDocumentProvider;
    private readonly AiGatewayOptions options;

    public RiskAnalysisSkill(
        IApplicationDocumentProvider applicationDocumentProvider,
        IAiGateway aiGateway,
        IOptions<AiGatewayOptions> options)
    {
        this.applicationDocumentProvider = applicationDocumentProvider;
        this.aiGateway = aiGateway;
        this.options = options.Value;
    }

    public RiskAnalysisSkillResponse? Run(RiskAnalysisSkillRequest request)
    {
        // Synchronous execution is the deterministic path used by harness checks and simple composition.
        var document = applicationDocumentProvider.FindById(request.DocumentId);
        if (document is null)
        {
            return null;
        }

        return BuildDeterministicRiskAnalysis(document);
    }

    public async Task<RiskAnalysisSkillResponse?> RunAsync(
        RiskAnalysisSkillRequest request,
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
            // Mock keeps risk analysis deterministic for local workflow and harness checks.
            return BuildDeterministicRiskAnalysis(document);
        }

        var prompt = DocumentSkillPromptTemplates.BuildRiskAnalysisPrompt(document);
        var modelResponse = await aiGateway.GenerateChatResponseAsync(
            new ChatModelRequest(prompt, provider),
            cancellationToken);

        // Parse model-generated risk objects, then fall back to deterministic signals if the output is unusable.
        return TryParseModelRiskAnalysis(document, modelResponse.Message.Answer)
            ?? BuildDeterministicRiskAnalysis(document);
    }

    private static RiskAnalysisSkillResponse BuildDeterministicRiskAnalysis(DocumentItemResponse document)
    {
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

    private static RiskAnalysisSkillResponse? TryParseModelRiskAnalysis(
        DocumentItemResponse document,
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

            return new RiskAnalysisSkillResponse(
                document.Id,
                document.Title,
                ReadRiskItems(root),
                SkillJsonReader.ReadStringArray(root, "missingInformation"));
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static IReadOnlyList<RiskItem> ReadRiskItems(JsonElement root)
    {
        if (!root.TryGetProperty("risks", out var risksProperty)
            || risksProperty.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return risksProperty
            .EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.Object)
            .Select(item => new RiskItem(
                SkillJsonReader.ReadString(item, "title", "Document risk"),
                NormalizeSeverity(SkillJsonReader.ReadString(item, "severity", "medium")),
                SkillJsonReader.ReadString(item, "source", "Document context"),
                SkillJsonReader.ReadString(item, "recommendation", "Review this item with the business owner.")))
            .ToArray();
    }

    private static string NormalizeSeverity(string severity)
    {
        return severity.ToLowerInvariant() switch
        {
            "high" => "high",
            "low" => "low",
            _ => "medium"
        };
    }

    private static IEnumerable<RiskItem> AnalyzeSection(DocumentSectionResponse section)
    {
        var text = $"{section.Title} {section.Body}";

        // Deterministic signals keep the Mock path useful without calling a model.
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
