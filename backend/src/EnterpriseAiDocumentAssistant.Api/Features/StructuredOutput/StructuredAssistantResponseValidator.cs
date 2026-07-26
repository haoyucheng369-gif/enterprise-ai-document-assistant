using EnterpriseAiDocumentAssistant.Api.Contracts;

namespace EnterpriseAiDocumentAssistant.Api.StructuredOutput;

public sealed class StructuredAssistantResponseValidator : IStructuredAssistantResponseValidator
{
    private static readonly HashSet<string> AllowedConfidenceValues = new(
        ["low", "medium", "high"],
        StringComparer.OrdinalIgnoreCase);

    public StructuredOutputValidationResult Validate(StructuredAssistantMessage message)
    {
        // This is the output-side contract check before a model response is returned to the frontend.
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(message.Answer))
        {
            errors.Add("Answer is required.");
        }

        if (!AllowedConfidenceValues.Contains(message.Confidence))
        {
            errors.Add("Confidence must be one of: low, medium, high.");
        }

        if (message.Citations is null)
        {
            // Empty citations are allowed, but the property itself must be present for stable UI rendering.
            errors.Add("Citations must be an array.");
        }

        if (message.SuggestedActions is null)
        {
            // Suggested actions are optional in meaning, but kept as an array in the API contract.
            errors.Add("SuggestedActions must be an array.");
        }

        return errors.Count == 0
            ? StructuredOutputValidationResult.Success
            : StructuredOutputValidationResult.Failure(errors);
    }
}
