namespace EnterpriseAiDocumentAssistant.Api.Guardrails;

public sealed record SafetyClassification(
    string Decision,
    string RiskType,
    string Reason,
    double Confidence,
    IReadOnlyList<string> Signals)
{
    public bool IsBlocked => string.Equals(Decision, "blocked", StringComparison.OrdinalIgnoreCase);

    public bool NeedsReview => string.Equals(Decision, "needs_review", StringComparison.OrdinalIgnoreCase);

    public static SafetyClassification Safe(string reason)
    {
        return new SafetyClassification("safe", "none", reason, 0.9, []);
    }

    public static SafetyClassification NeedsHumanReview(
        string riskType,
        string reason,
        IReadOnlyList<string> signals)
    {
        return new SafetyClassification("needs_review", riskType, reason, 0.7, signals);
    }

    public static SafetyClassification Blocked(
        string riskType,
        string reason,
        IReadOnlyList<string> signals)
    {
        return new SafetyClassification("blocked", riskType, reason, 0.95, signals);
    }
}
