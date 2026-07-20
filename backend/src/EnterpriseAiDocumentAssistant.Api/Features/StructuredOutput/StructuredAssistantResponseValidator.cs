using EnterpriseAiDocumentAssistant.Api.Contracts;

namespace EnterpriseAiDocumentAssistant.Api.StructuredOutput;

public sealed class StructuredAssistantResponseValidator : IStructuredAssistantResponseValidator
{
    private static readonly HashSet<string> AllowedConfidenceValues = new(
        ["low", "medium", "high"],
        StringComparer.OrdinalIgnoreCase);

    public StructuredOutputValidationResult Validate(StructuredAssistantMessage message)
    {
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
            errors.Add("Citations must be an array.");
        }

        if (message.SuggestedActions is null)
        {
            errors.Add("SuggestedActions must be an array.");
        }

        return errors.Count == 0
            ? StructuredOutputValidationResult.Success
            : StructuredOutputValidationResult.Failure(errors);
    }
}
