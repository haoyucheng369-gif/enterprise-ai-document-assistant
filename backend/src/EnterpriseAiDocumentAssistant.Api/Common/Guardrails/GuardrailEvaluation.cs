using EnterpriseAiDocumentAssistant.Api.Contracts;

namespace EnterpriseAiDocumentAssistant.Api.Guardrails;

public sealed record GuardrailEvaluation(
    bool IsBlocked,
    string? Reason,
    StructuredAssistantMessage? Response,
    SafetyClassification Classification)
{
    public static GuardrailEvaluation Allowed(SafetyClassification classification)
    {
        return new GuardrailEvaluation(false, null, null, classification);
    }

    public static GuardrailEvaluation Blocked(
        string reason,
        StructuredAssistantMessage response,
        SafetyClassification classification)
    {
        return new GuardrailEvaluation(true, reason, response, classification);
    }
}
