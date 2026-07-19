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

        var provider = ResolveProvider(request.AiProvider);
        if (IsMockProvider(provider))
        {
            // The mock path keeps classification debuggable and cost-free while preserving the real response shape.
            return BuildDeterministicClassification(document, provider);
        }

        // The real-provider path asks the model for a compact JSON object, then maps it into the skill contract.
        var prompt = BuildPrompt(document);
        var modelResponse = await aiGateway.GenerateChatResponseAsync(
            new ChatModelRequest(prompt, provider),
            cancellationToken);

        return TryParseModelClassification(document, modelResponse.Message.Answer, provider)
            ?? BuildFallbackClassification(document, provider, modelResponse.Message.Answer);
    }

    private string ResolveProvider(string? requestedProvider)
    {
        return string.IsNullOrWhiteSpace(requestedProvider)
            ? options.Provider
            : requestedProvider.Trim();
    }

    private static OrchestratedPrompt BuildPrompt(DocumentItemResponse document)
    {
        var documentText = string.Join(
            Environment.NewLine,
            document.Sections.Select(section =>
                $"{section.Label} - {section.Title}: {section.Body}"));

        if (string.IsNullOrWhiteSpace(documentText))
        {
            documentText = $"{document.Type} document titled {document.Title}.";
        }

        var truncatedDocumentText = Truncate(documentText, 6000);
        var variables = new[]
        {
            new PromptVariable("document_title", document.Title),
            new PromptVariable("document_type", document.Type),
            new PromptVariable("document_text", truncatedDocumentText)
        };

        var userMessage = $"""
            Classify this document.

            Title: {document.Title}
            Type hint: {document.Type}

            Document text:
            {truncatedDocumentText}
            """;

        // Classification prompts force a compact object so application code can validate and store it.
        return new OrchestratedPrompt(
            "document-classification",
            "You classify enterprise documents for a document assistant. Keep the classification practical and conservative.",
            userMessage,
            [
                "Set answer to a single minified JSON object with category, priority, confidence, reason, signals, and sources.",
                "Allowed category values: Contract, Policy, Report, Resume, TechnicalDocument, Other.",
                "Allowed priority values: Low, Medium, High.",
                "Confidence must be a number from 0 to 1.",
                "Signals and sources must be arrays of short strings."
            ],
            variables);
    }

    private static ClassificationSkillResponse BuildDeterministicClassification(
        DocumentItemResponse document,
        string provider)
    {
        var text = $"{document.Title} {document.Type} {string.Join(' ', document.Sections.Select(section => $"{section.Title} {section.Body}"))}";
        var sources = GetTopSources(document);

        if (ContainsAny(text, ["agreement", "liability", "renewal", "service credit", "vendor"]))
        {
            return new ClassificationSkillResponse(
                document.Id,
                document.Title,
                "Contract",
                "High",
                0.82,
                "The document contains contract-style terms such as renewal, liability, vendor, or service credit language.",
                ["renewal/liability/service credit signals"],
                sources,
                provider);
        }

        if (ContainsAny(text, ["policy", "security", "data protection", "access"]))
        {
            return new ClassificationSkillResponse(
                document.Id,
                document.Title,
                "Policy",
                "Medium",
                0.78,
                "The document contains policy and control language.",
                ["policy/security/access signals"],
                sources,
                provider);
        }

        if (ContainsAny(text, ["report", "q1", "q2", "q3", "q4", "operations"]))
        {
            return new ClassificationSkillResponse(
                document.Id,
                document.Title,
                "Report",
                "Medium",
                0.74,
                "The document looks like an operational report.",
                ["reporting/operations signals"],
                sources,
                provider);
        }

        if (ContainsAny(text, ["developer", "experience", "skills", "linkedin", "github"]))
        {
            return new ClassificationSkillResponse(
                document.Id,
                document.Title,
                "Resume",
                "Low",
                0.76,
                "The document contains professional profile and experience signals.",
                ["profile/experience/skills signals"],
                sources,
                provider);
        }

        return new ClassificationSkillResponse(
            document.Id,
            document.Title,
            "Other",
            "Low",
            0.55,
            "No strong enterprise document category signal was found.",
            [],
            sources,
            provider);
    }

    private static ClassificationSkillResponse? TryParseModelClassification(
        DocumentItemResponse document,
        string answer,
        string provider)
    {
        try
        {
            using var jsonDocument = JsonDocument.Parse(answer);
            var root = jsonDocument.RootElement;

            return new ClassificationSkillResponse(
                document.Id,
                document.Title,
                ReadString(root, "category", "Other"),
                NormalizePriority(ReadString(root, "priority", "Low")),
                Clamp(ReadDouble(root, "confidence", 0.5), 0, 1),
                ReadString(root, "reason", "The model returned a classification without a detailed reason."),
                ReadStringArray(root, "signals"),
                ReadStringArray(root, "sources"),
                provider);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static ClassificationSkillResponse BuildFallbackClassification(
        DocumentItemResponse document,
        string provider,
        string modelAnswer)
    {
        return new ClassificationSkillResponse(
            document.Id,
            document.Title,
            "Other",
            "Low",
            0.5,
            string.IsNullOrWhiteSpace(modelAnswer)
                ? "The model did not return a parseable classification."
                : modelAnswer,
            [],
            GetTopSources(document),
            provider);
    }

    private static IReadOnlyList<string> GetTopSources(DocumentItemResponse document)
    {
        return document.Sections
            .Select(section => section.Label)
            .Take(3)
            .ToArray();
    }

    private static bool IsMockProvider(string provider)
    {
        return string.Equals(provider, "Mock", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsAny(string value, IReadOnlyList<string> signals)
    {
        return signals.Any(signal =>
            value.Contains(signal, StringComparison.OrdinalIgnoreCase));
    }

    private static string ReadString(JsonElement root, string propertyName, string fallback)
    {
        return root.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(property.GetString())
                ? property.GetString()!
                : fallback;
    }

    private static double ReadDouble(JsonElement root, string propertyName, double fallback)
    {
        return root.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.Number
            && property.TryGetDouble(out var value)
                ? value
                : fallback;
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var property)
            || property.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return property
            .EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToArray();
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

    private static double Clamp(double value, double minimum, double maximum)
    {
        return Math.Min(maximum, Math.Max(minimum, value));
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength
            ? value
            : value[..maxLength];
    }
}
