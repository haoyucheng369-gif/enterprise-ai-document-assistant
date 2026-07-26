using System.Text.Json;
using EnterpriseAiDocumentAssistant.Api.Options;

namespace EnterpriseAiDocumentAssistant.Api.Skills;

internal static class SkillJsonReader
{
    // Skills share provider resolution so Mock/OpenAI/Azure behavior stays consistent across endpoints.
    public static string ResolveProvider(AiGatewayOptions options, string? requestedProvider)
    {
        return string.IsNullOrWhiteSpace(requestedProvider)
            ? options.Provider
            : requestedProvider.Trim();
    }

    public static bool IsMockProvider(string provider)
    {
        // A single Mock check prevents every skill from duplicating provider string comparisons.
        return string.Equals(provider, "Mock", StringComparison.OrdinalIgnoreCase);
    }

    // Model answers are JSON strings inside the AI Gateway response; these helpers keep parsing code small.
    public static string ReadString(JsonElement root, string propertyName, string fallback)
    {
        return root.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(property.GetString())
                ? property.GetString()!
                : fallback;
    }

    public static double ReadDouble(JsonElement root, string propertyName, double fallback)
    {
        // Numeric fields such as confidence are optional and fall back when the model omits them.
        return root.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.Number
            && property.TryGetDouble(out var value)
                ? value
                : fallback;
    }

    public static IReadOnlyList<string> ReadStringArray(JsonElement root, string propertyName)
    {
        // Arrays from model output are filtered to strings so downstream code never handles mixed JSON types.
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
}
