using EnterpriseAiDocumentAssistant.Api.Contracts;

namespace EnterpriseAiDocumentAssistant.Api.PromptOrchestration;

public interface IDocumentAssistantPromptOrchestrator
{
    OrchestratedPrompt BuildPrompt(ChatRequest request);

    StructuredAssistantMessage BuildMockStructuredResponse(OrchestratedPrompt prompt);

    IEnumerable<string> BuildMockResponseChunks(StructuredAssistantMessage message);
}
