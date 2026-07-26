using EnterpriseAiDocumentAssistant.Api.Contracts;

namespace EnterpriseAiDocumentAssistant.Api.Guardrails;

public sealed class ChatGuardrailEvaluator : IChatGuardrailEvaluator
{
    private readonly ISafetyClassifier safetyClassifier;

    public ChatGuardrailEvaluator(ISafetyClassifier safetyClassifier)
    {
        this.safetyClassifier = safetyClassifier;
    }

    public GuardrailEvaluation Evaluate(ChatRequest request)
    {
        // This first guardrail is intentionally simple: classify obvious unsafe requests before model execution.
        // It is not a full security system; authorization and RAG filtering must be added later.
        var classification = safetyClassifier.Classify(request);
        return BuildEvaluation(classification);
    }

    public async Task<GuardrailEvaluation> EvaluateAsync(
        ChatRequest request,
        CancellationToken cancellationToken)
    {
        // Async evaluation can include an AI-backed safety classifier when a real provider is selected.
        var classification = await safetyClassifier.ClassifyAsync(request, cancellationToken);
        return BuildEvaluation(classification);
    }

    private static GuardrailEvaluation BuildEvaluation(SafetyClassification classification)
    {
        if (classification.IsBlocked && classification.RiskType == "prompt_injection")
        {
            return GuardrailEvaluation.Blocked(
                "PromptInjectionAttempt",
                new StructuredAssistantMessage(
                    "I cannot ignore the application instructions or reveal hidden/system prompts. I can still help with questions that use the selected document context and allowed tools.",
                    "low",
                    [],
                    [
                        "Ask a document-specific question.",
                        "Use approved tools for document metadata or health status.",
                        "Avoid requests that try to override system instructions."
                    ]),
                classification);
        }

        if (classification.IsBlocked && classification.RiskType == "unauthorized_data")
        {
            return GuardrailEvaluation.Blocked(
                "UnauthorizedDataRequest",
                new StructuredAssistantMessage(
                    "I cannot help retrieve confidential, secret, or unauthorized information. I can only work with the document context and capabilities exposed by this application.",
                    "low",
                    [],
                    [
                        "Ask about the selected document.",
                        "Use authorized document metadata or search tools when available.",
                        "Request access through the appropriate business process if more data is needed."
                    ]),
                classification);
        }

        return GuardrailEvaluation.Allowed(classification);
    }
}
