using EnterpriseAiDocumentAssistant.Api.Contracts;

namespace EnterpriseAiDocumentAssistant.Api.Guardrails;

public interface IChatGuardrailEvaluator
{
    GuardrailEvaluation Evaluate(ChatRequest request);
}
