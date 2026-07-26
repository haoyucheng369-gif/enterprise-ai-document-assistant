using EnterpriseAiDocumentAssistant.Api.Contracts;

namespace EnterpriseAiDocumentAssistant.Api.Guardrails;

public sealed class RuleBasedSafetyClassifier : ISafetyClassifier
{
    private static readonly string[] PromptInjectionSignals =
    [
        "ignore previous instructions",
        "ignore all previous instructions",
        "ignore the system prompt",
        "forget your instructions",
        "override your instructions",
        "reveal your system prompt",
        "show me your hidden prompt",
        "show system prompt",
        "jailbreak"
    ];

    private static readonly string[] UnauthorizedDataSignals =
    [
        "confidential",
        "secret",
        "payroll",
        "private key",
        "access token",
        "all internal files",
        "documents i do not have access to"
    ];

    private static readonly string[] ReviewSignals =
    [
        "bypass",
        "hidden instruction",
        "developer message",
        "internal policy",
        "salary"
    ];

    public SafetyClassification Classify(ChatRequest request)
    {
        // Safety classification creates a structured decision before planner or model execution.
        var message = request.Message.Trim();

        var promptInjectionMatches = FindMatches(message, PromptInjectionSignals);
        if (promptInjectionMatches.Count > 0)
        {
            return SafetyClassification.Blocked(
                "prompt_injection",
                "The request attempts to override or reveal assistant instructions.",
                promptInjectionMatches);
        }

        var unauthorizedDataMatches = FindMatches(message, UnauthorizedDataSignals);
        if (unauthorizedDataMatches.Count > 0)
        {
            return SafetyClassification.Blocked(
                "unauthorized_data",
                "The request asks for confidential or unauthorized information.",
                unauthorizedDataMatches);
        }

        var reviewMatches = FindMatches(message, ReviewSignals);
        if (reviewMatches.Count > 0)
        {
            // Needs-review requests are allowed in V1, but they are visible for audit and future policy decisions.
            return SafetyClassification.NeedsHumanReview(
                "suspicious_request",
                "The request contains terms that may require stronger policy handling.",
                reviewMatches);
        }

        return SafetyClassification.Safe("No safety signal matched.");
    }

    public Task<SafetyClassification> ClassifyAsync(
        ChatRequest request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Classify(request));
    }

    private static IReadOnlyList<string> FindMatches(string value, IReadOnlyList<string> signals)
    {
        // Return matched signals so logs and harness output explain why a decision was made.
        return signals
            .Where(signal => value.Contains(signal, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }
}
