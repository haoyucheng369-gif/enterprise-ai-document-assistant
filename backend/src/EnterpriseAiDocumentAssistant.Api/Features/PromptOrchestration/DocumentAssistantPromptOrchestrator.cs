using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.ConversationMemory;
using EnterpriseAiDocumentAssistant.Api.Services;

namespace EnterpriseAiDocumentAssistant.Api.PromptOrchestration;

public sealed class DocumentAssistantPromptOrchestrator : IDocumentAssistantPromptOrchestrator
{
    private const int MaxDocumentSections = 4;
    private const int MaxSectionLength = 1200;

    private readonly IApplicationDocumentProvider applicationDocumentProvider;
    private readonly IConversationMemoryBuilder conversationMemoryBuilder;

    public DocumentAssistantPromptOrchestrator(
        IConversationMemoryBuilder conversationMemoryBuilder,
        IApplicationDocumentProvider applicationDocumentProvider)
    {
        this.conversationMemoryBuilder = conversationMemoryBuilder;
        this.applicationDocumentProvider = applicationDocumentProvider;
    }

    public OrchestratedPrompt BuildAssistantPrompt(ChatRequest request)
    {
        var userQuestion = request.Message.Trim();
        var memory = conversationMemoryBuilder.Build(request.History);
        var documentContext = BuildDocumentContext(request.DocumentId);

        // Prompt orchestration combines the current question, selected document content, and recent turns.
        var variables = DocumentAssistantPrompt.BuildVariables(
            userQuestion,
            documentContext,
            memory.Turns.Count,
            memory.PromptText);

        return new OrchestratedPrompt(
            DocumentAssistantPrompt.Template.Name,
            DocumentAssistantPrompt.Template.SystemMessage,
            RenderTemplate(DocumentAssistantPrompt.Template.UserMessageTemplate, variables),
            DocumentAssistantPrompt.Template.OutputRules,
            variables);
    }

    private string BuildDocumentContext(string? documentId)
    {
        if (string.IsNullOrWhiteSpace(documentId))
        {
            return "No document is selected.";
        }

        var document = applicationDocumentProvider.FindById(documentId);
        if (document is null)
        {
            return $"Selected document id: {documentId}. Document content was not found.";
        }

        var sections = document.Sections
            .Take(MaxDocumentSections)
            .Select(section =>
                $"{section.Label} - {section.Title}: {Truncate(section.Body, MaxSectionLength)}");

        return $"""
            Selected document id: {document.Id}
            Title: {document.Title}
            Type: {document.Type}
            Status: {document.Status}
            Content:
            {string.Join(Environment.NewLine, sections)}
            """;
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

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength
            ? value
            : value[..maxLength];
    }

}
