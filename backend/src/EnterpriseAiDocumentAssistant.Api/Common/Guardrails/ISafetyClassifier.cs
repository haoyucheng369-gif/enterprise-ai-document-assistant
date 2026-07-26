using EnterpriseAiDocumentAssistant.Api.Contracts;

namespace EnterpriseAiDocumentAssistant.Api.Guardrails;

public interface ISafetyClassifier
{
    SafetyClassification Classify(ChatRequest request);

    Task<SafetyClassification> ClassifyAsync(
        ChatRequest request,
        CancellationToken cancellationToken);
}
