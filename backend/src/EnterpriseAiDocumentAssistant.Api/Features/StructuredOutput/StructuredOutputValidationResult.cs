namespace EnterpriseAiDocumentAssistant.Api.StructuredOutput;

public sealed record StructuredOutputValidationResult(
    bool IsValid,
    IReadOnlyList<string> Errors)
{
    public static StructuredOutputValidationResult Success { get; } = new(true, []);

    public static StructuredOutputValidationResult Failure(IReadOnlyList<string> errors)
    {
        return new StructuredOutputValidationResult(false, errors);
    }
}
