using EnterpriseAiDocumentAssistant.Api.Contracts;

namespace EnterpriseAiDocumentAssistant.Api.PromptOrchestration;

public interface IDocumentAssistantPromptOrchestrator
{
    OrchestratedPrompt BuildAssistantPrompt(ChatRequest request);
}
