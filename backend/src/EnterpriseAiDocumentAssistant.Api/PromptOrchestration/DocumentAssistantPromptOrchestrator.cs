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

    public StructuredAssistantMessage BuildMockStructuredResponse(OrchestratedPrompt prompt)
    {
        var documentContext = GetVariable(prompt, "document_context");
        var userQuestion = GetVariable(prompt, "user_question");

        return new StructuredAssistantMessage(
            $"Using prompt template '{prompt.TemplateName}', I am reviewing {documentContext}. Your question was: \"{userQuestion}\". The request now flows through prompt orchestration and structured output validation before model integration.",
            "medium",
            [],
            [
                "Review the structured response contract.",
                "Use citations when RAG retrieval is added.",
                "Route this response through the AI Gateway in a later step."
            ]);
    }

    public IEnumerable<string> BuildMockResponseChunks(StructuredAssistantMessage message)
    {
        yield return message.Answer;
        yield return $" Confidence: {message.Confidence}.";

        if (message.SuggestedActions.Count > 0)
        {
            yield return $" Suggested action: {message.SuggestedActions[0]}";
        }
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

    private static string GetVariable(OrchestratedPrompt prompt, string name)
    {
        return prompt.Variables
            .First(variable => string.Equals(variable.Name, name, StringComparison.Ordinal))
            .Value;
    }
}
