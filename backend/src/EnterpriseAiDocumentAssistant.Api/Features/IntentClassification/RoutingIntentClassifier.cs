using System.Text.Json;
using EnterpriseAiDocumentAssistant.Api.Audit;

namespace EnterpriseAiDocumentAssistant.Api.IntentClassification;

public sealed class RoutingIntentClassifier : IIntentClassifier
{
    private readonly AiIntentClassifier aiClassifier;
    private readonly RuleBasedIntentClassifier ruleBasedClassifier;
    private readonly IAuditLogger auditLogger;

    public RoutingIntentClassifier(
        AiIntentClassifier aiClassifier,
        RuleBasedIntentClassifier ruleBasedClassifier,
        IAuditLogger auditLogger)
    {
        this.aiClassifier = aiClassifier;
        this.ruleBasedClassifier = ruleBasedClassifier;
        this.auditLogger = auditLogger;
    }

    public IntentClassificationResult Classify(IntentClassificationRequest request)
    {
        // Synchronous callers use deterministic classification, mainly for harness checks.
        var result = ruleBasedClassifier.Classify(request);
        Record(result);
        return result;
    }

    public async Task<IntentClassificationResult> ClassifyAsync(
        IntentClassificationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var aiResult = await aiClassifier.TryClassifyAsync(request, cancellationToken);
            if (aiResult is not null)
            {
                Record(aiResult);
                return aiResult;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (
            ex is InvalidOperationException
                or HttpRequestException
                or TaskCanceledException
                or JsonException)
        {
            RecordFallback(ex.GetType().Name);
        }

        // Invalid or unavailable model classification falls back to predictable local rules.
        var fallbackResult = ruleBasedClassifier.Classify(request);
        Record(fallbackResult);
        return fallbackResult;
    }

    private void Record(IntentClassificationResult result)
    {
        auditLogger.Record(new AuditEventRequest(
            "intent",
            "intent_classified",
            result.Intent,
            true,
            0,
            new Dictionary<string, string>
            {
                ["source"] = result.Source,
                ["reason"] = result.Reason
            }));
    }

    private void RecordFallback(string reason)
    {
        auditLogger.Record(new AuditEventRequest(
            "intent",
            "ai_classification_fallback",
            "intent.routing",
            false,
            0,
            new Dictionary<string, string>
            {
                ["reason"] = reason
            }));
    }
}
