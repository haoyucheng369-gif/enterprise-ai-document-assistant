using EnterpriseAiDocumentAssistant.Api.Contracts;

namespace EnterpriseAiDocumentAssistant.Api.PromptOrchestration;

public interface IDocumentAssistantPromptOrchestrator
{
    OrchestratedPrompt BuildPrompt(ChatRequest request);

    IEnumerable<string> BuildMockResponseChunks(OrchestratedPrompt prompt);
}
