namespace EnterpriseAiDocumentAssistant.Api.PromptOrchestration;

public sealed record PromptTemplate(
    string Name,
    string SystemMessage,
    string UserMessageTemplate,
    IReadOnlyList<string> OutputRules);

public sealed record PromptVariable(
    string Name,
    string Value);

public sealed record OrchestratedPrompt(
    string TemplateName,
    string SystemMessage,
    string UserMessage,
    IReadOnlyList<string> OutputRules,
    IReadOnlyList<PromptVariable> Variables);
