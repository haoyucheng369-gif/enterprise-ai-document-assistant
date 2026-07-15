using EnterpriseAiDocumentAssistant.Api.Contracts;

namespace EnterpriseAiDocumentAssistant.Api.PromptOrchestration;

public sealed class DocumentAssistantPromptOrchestrator : IDocumentAssistantPromptOrchestrator
{
    public OrchestratedPrompt BuildPrompt(ChatRequest request)
    {
        var userQuestion = request.Message.Trim();
        var variables = DocumentAssistantPrompt.BuildVariables(
            userQuestion,
            request.DocumentId,
            request.History?.Count ?? 0);

        return new OrchestratedPrompt(
            DocumentAssistantPrompt.Template.Name,
            DocumentAssistantPrompt.Template.SystemMessage,
            RenderTemplate(DocumentAssistantPrompt.Template.UserMessageTemplate, variables),
            DocumentAssistantPrompt.Template.OutputRules,
            variables);
    }

    public IEnumerable<string> BuildMockResponseChunks(OrchestratedPrompt prompt)
    {
        var documentContext = GetVariable(prompt, "document_context");
        var userQuestion = GetVariable(prompt, "user_question");

        yield return $"Using prompt template '{prompt.TemplateName}'. ";
        yield return $"I am reviewing {documentContext}. ";
        yield return $"Your question was: \"{userQuestion}\". ";
        yield return "The request now flows through prompt orchestration before model integration. ";
        yield return "The same prompt envelope can be passed to the AI Gateway in a later step.";
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
