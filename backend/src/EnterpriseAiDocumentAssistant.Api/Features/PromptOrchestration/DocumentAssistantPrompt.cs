namespace EnterpriseAiDocumentAssistant.Api.PromptOrchestration;

public static class DocumentAssistantPrompt
{
    public static readonly PromptTemplate Template = new(
        "document-assistant-v1",
        EnterpriseAssistantPromptDefaults.BuildSystemMessage(
            "Answer user questions using the selected document context when available."),
        """
        Document context: {{document_context}}
        Conversation turns: {{conversation_turn_count}}
        Recent conversation:
        {{conversation_memory}}
        User question: {{user_question}}
        """,
        EnterpriseAssistantPromptDefaults.CombineOutputRules(
            EnterpriseAssistantPromptDefaults.OutputRules,
            [
                "Answer the user question directly.",
                "Mention when document context is missing or limited.",
                "Prepare the response so citations can be attached in a later RAG step.",
                "Suggested actions must be short user-facing commands, not assistant-perspective statements.",
                "Suggested actions must be written as direct user commands, not assistant-offer phrases.",
                "Good suggested action examples: 'Summarize key points', 'Extract the skill list', 'Review risk items'."
            ]));

    public static IReadOnlyList<PromptVariable> BuildVariables(
        string userQuestion,
        string documentContext,
        int conversationTurnCount,
        string conversationMemory)
    {
        return
        [
            new PromptVariable("document_context", documentContext),
            new PromptVariable("conversation_turn_count", conversationTurnCount.ToString()),
            new PromptVariable("conversation_memory", conversationMemory),
            new PromptVariable("user_question", userQuestion)
        ];
    }
}
