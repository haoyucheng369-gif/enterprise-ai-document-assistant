namespace EnterpriseAiDocumentAssistant.Api.IntentClassification;

public interface IIntentClassifier
{
    IntentClassificationResult Classify(IntentClassificationRequest request);

    Task<IntentClassificationResult> ClassifyAsync(
        IntentClassificationRequest request,
        CancellationToken cancellationToken);
}
