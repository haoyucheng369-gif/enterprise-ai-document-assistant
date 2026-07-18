using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.ConversationMemory;

namespace EnterpriseAiDocumentAssistant.Api.PromptOrchestration;

public sealed class DocumentAssistantPromptOrchestrator : IDocumentAssistantPromptOrchestrator
{
    private readonly IConversationMemoryBuilder conversationMemoryBuilder;

    public DocumentAssistantPromptOrchestrator(IConversationMemoryBuilder conversationMemoryBuilder)
    {
        this.conversationMemoryBuilder = conversationMemoryBuilder;
    }

    public OrchestratedPrompt BuildPrompt(ChatRequest request)
    {
        var userQuestion = request.Message.Trim();
        var memory = conversationMemoryBuilder.Build(request.History);

        // Prompt orchestration combines the current question, document target, and recent turns.
        var variables = DocumentAssistantPrompt.BuildVariables(
            userQuestion,
            request.DocumentId,
            memory.Turns.Count,
            memory.PromptText);

        return new OrchestratedPrompt(
            DocumentAssistantPrompt.Template.Name,
            DocumentAssistantPrompt.Template.SystemMessage,
            RenderTemplate(DocumentAssistantPrompt.Template.UserMessageTemplate, variables),
            DocumentAssistantPrompt.Template.OutputRules,
            variables);
    }

    private static string RenderTemplate(
        string template,
        IReadOnlyList<PromptVariable> variables)
    {
        var renderedTemplate = template;

        foreach (var variable in variables)
        {
            renderedTemplate = renderedTemplate.Replace(
                $"{{{{{variable.Name}}}}}",
                variable.Value,
                StringComparison.Ordinal);
        }

        return renderedTemplate;
    }

}
