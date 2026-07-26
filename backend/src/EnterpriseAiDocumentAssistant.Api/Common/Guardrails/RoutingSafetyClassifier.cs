using EnterpriseAiDocumentAssistant.Api.Contracts;

namespace EnterpriseAiDocumentAssistant.Api.Guardrails;

public sealed class RoutingSafetyClassifier : ISafetyClassifier
{
    private readonly AiSafetyClassifier aiSafetyClassifier;
    private readonly RuleBasedSafetyClassifier ruleBasedSafetyClassifier;

    public RoutingSafetyClassifier(
        RuleBasedSafetyClassifier ruleBasedSafetyClassifier,
        AiSafetyClassifier aiSafetyClassifier)
    {
        this.ruleBasedSafetyClassifier = ruleBasedSafetyClassifier;
        this.aiSafetyClassifier = aiSafetyClassifier;
    }

    public SafetyClassification Classify(ChatRequest request)
    {
        // Synchronous callers get the deterministic rule-based classifier.
        return ruleBasedSafetyClassifier.Classify(request);
    }

    public async Task<SafetyClassification> ClassifyAsync(
        ChatRequest request,
        CancellationToken cancellationToken)
    {
        var ruleDecision = ruleBasedSafetyClassifier.Classify(request);
        if (ruleDecision.IsBlocked)
        {
            // Clear policy violations are blocked before spending model tokens.
            return ruleDecision;
        }

        try
        {
            var aiDecision = await aiSafetyClassifier.TryClassifyAsync(request, cancellationToken);
            if (aiDecision is null)
            {
                return ruleDecision;
            }

            // Keep the conservative rule decision if rules found a needs-review signal.
            return ruleDecision.NeedsReview && !aiDecision.IsBlocked
                ? ruleDecision
                : aiDecision;
        }
        catch
        {
            // Safety must fail closed to the deterministic rule result when provider calls fail.
            return ruleDecision;
        }
    }
}
