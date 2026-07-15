using EnterpriseAiDocumentAssistant.Api.Contracts;

namespace EnterpriseAiDocumentAssistant.Api.Guardrails;

public sealed record GuardrailEvaluation(
    bool IsBlocked,
    string? Reason,
    StructuredAssistantMessage? Response)
{
    public static GuardrailEvaluation Allowed { get; } = new(false, null, null);

    public static GuardrailEvaluation Blocked(
        string reason,
        StructuredAssistantMessage response)
    {
        return new GuardrailEvaluation(true, reason, response);
    }
}
