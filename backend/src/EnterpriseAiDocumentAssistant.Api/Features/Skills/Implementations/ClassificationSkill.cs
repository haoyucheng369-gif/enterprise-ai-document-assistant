using System.Text.Json;
using EnterpriseAiDocumentAssistant.Api.AiGateway;
using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.Options;
using EnterpriseAiDocumentAssistant.Api.PromptOrchestration;
using EnterpriseAiDocumentAssistant.Api.Services;
using Microsoft.Extensions.Options;

namespace EnterpriseAiDocumentAssistant.Api.Skills;

public sealed class ClassificationSkill : IClassificationSkill
{
    private readonly IAiGateway aiGateway;
    private readonly IApplicationDocumentProvider applicationDocumentProvider;
    private readonly AiGatewayOptions options;

    public ClassificationSkill(
        IApplicationDocumentProvider applicationDocumentProvider,
        IAiGateway aiGateway,
        IOptions<AiGatewayOptions> options)
    {
        this.applicationDocumentProvider = applicationDocumentProvider;
        this.aiGateway = aiGateway;
        this.options = options.Value;
    }

    public async Task<ClassificationSkillResponse?> RunAsync(
        ClassificationSkillRequest request,
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
            // Mock keeps classification debuggable and cost-free while preserving the real response shape.
            return BuildDeterministicClassification(document, provider);
        }

        // Real providers return a compact classification object that is mapped into the skill contract.
        var prompt = DocumentSkillPromptTemplates.BuildClassificationPrompt(document);
        var modelResponse = await aiGateway.GenerateChatResponseAsync(
            new ChatModelRequest(prompt, provider),
            cancellationToken);

        return TryMapCategoryOnlyAnswer(document, modelResponse.Message.Answer, provider)
            ?? TryParseModelClassification(document, modelResponse.Message.Answer, provider)
            ?? BuildFallbackClassification(document, provider, modelResponse.Message.Answer);
    }

    private static ClassificationSkillResponse BuildDeterministicClassification(
        DocumentItemResponse document,
        string provider)
    {
        // Fallback classifier uses transparent keyword signals, useful for debugging route and UI behavior.
        var text = $"{document.Title} {document.Type} {string.Join(' ', document.Sections.Select(section => $"{section.Title} {section.Body}"))}";

        if (ContainsAny(text, ["agreement", "liability", "renewal", "service credit", "vendor"]))
        {
            return BuildResponse(
                document,
                provider,
                "Contract",
                "High",
                0.82,
                "The document contains contract-style terms such as renewal, liability, vendor, or service credit language.",
                ["renewal/liability/service credit signals"]);
        }

        if (ContainsAny(text, ["policy", "security", "data protection", "access"]))
        {
            return BuildResponse(
                document,
                provider,
                "Policy",
                "Medium",
                0.78,
                "The document contains policy and control language.",
                ["policy/security/access signals"]);
        }

        if (ContainsAny(text, ["report", "q1", "q2", "q3", "q4", "operations"]))
        {
            return BuildResponse(
                document,
                provider,
                "Report",
                "Medium",
                0.74,
                "The document looks like an operational report.",
                ["reporting/operations signals"]);
        }

        if (ContainsAny(text, ["developer", "experience", "skills", "linkedin", "github"]))
        {
            return BuildResponse(
                document,
                provider,
                "Resume",
                "Low",
                0.76,
                "The document contains professional profile and experience signals.",
                ["profile/experience/skills signals"]);
        }

        return BuildResponse(
            document,
            provider,
            "Other",
            "Low",
            0.55,
            "No strong enterprise document category signal was found.",
            []);
    }

    private static ClassificationSkillResponse? TryParseModelClassification(
        DocumentItemResponse document,
        string answer,
        string provider)
    {
        // Real classification should be JSON so category, priority, confidence, and sources remain machine-readable.
        if (!answer.TrimStart().StartsWith('{'))
        {
            return null;
        }

        try
        {
            using var jsonDocument = JsonDocument.Parse(answer);
            var root = jsonDocument.RootElement;

            return BuildResponse(
                document,
                provider,
                SkillJsonReader.ReadString(root, "category", "Other"),
                NormalizePriority(SkillJsonReader.ReadString(root, "priority", "Low")),
                Clamp(SkillJsonReader.ReadDouble(root, "confidence", 0.5), 0, 1),
                SkillJsonReader.ReadString(root, "reason", "The model returned a classification without a detailed reason."),
                SkillJsonReader.ReadStringArray(root, "signals"),
                SkillJsonReader.ReadStringArray(root, "sources"));
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static ClassificationSkillResponse? TryMapCategoryOnlyAnswer(
        DocumentItemResponse document,
        string answer,
        string provider)
    {
        var category = NormalizeCategory(answer);
        if (category is null)
        {
            return null;
        }

        // Some models may return only the category label despite the prompt; keep that usable and visible.
        return BuildResponse(
            document,
            provider,
            category,
            category == "Contract" ? "Medium" : "Low",
            0.6,
            $"The model returned a category label only: {category}.",
            [$"category label: {category}"]);
    }

    private static ClassificationSkillResponse BuildFallbackClassification(
        DocumentItemResponse document,
        string provider,
        string modelAnswer)
    {
        return BuildResponse(
            document,
            provider,
            "Other",
            "Low",
            0.5,
            string.IsNullOrWhiteSpace(modelAnswer)
                ? "The model did not return a parseable classification."
                : modelAnswer,
            []);
    }

    private static ClassificationSkillResponse BuildResponse(
        DocumentItemResponse document,
        string provider,
        string category,
        string priority,
        double confidence,
        string reason,
        IReadOnlyList<string> signals,
        IReadOnlyList<string>? sources = null)
    {
        return new ClassificationSkillResponse(
            document.Id,
            document.Title,
            category,
            priority,
            confidence,
            reason,
            signals,
            sources?.Count > 0 ? sources : GetTopSources(document),
            provider);
    }

    private static IReadOnlyList<string> GetTopSources(DocumentItemResponse document)
    {
        return document.Sections
            .Select(section => section.Label)
            .Take(3)
            .ToArray();
    }

    private static bool ContainsAny(string value, IReadOnlyList<string> signals)
    {
        return signals.Any(signal =>
            value.Contains(signal, StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizePriority(string priority)
    {
        return priority.ToLowerInvariant() switch
        {
            "high" => "High",
            "medium" => "Medium",
            _ => "Low"
        };
    }

    private static string? NormalizeCategory(string category)
    {
        return category.Trim().Trim('"').ToLowerInvariant() switch
        {
            "contract" => "Contract",
            "policy" => "Policy",
            "report" => "Report",
            "resume" => "Resume",
            "technicaldocument" or "technical document" => "TechnicalDocument",
            "other" => "Other",
            _ => null
        };
    }

    private static double Clamp(double value, double minimum, double maximum)
    {
        return Math.Min(maximum, Math.Max(minimum, value));
    }

}
