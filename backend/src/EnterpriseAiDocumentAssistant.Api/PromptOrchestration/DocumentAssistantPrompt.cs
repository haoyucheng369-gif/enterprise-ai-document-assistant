namespace EnterpriseAiDocumentAssistant.Api.PromptOrchestration;

public static class DocumentAssistantPrompt
{
    public static readonly PromptTemplate Template = new(
        "document-assistant-v1",
        """
        You are an enterprise document assistant.
        Answer using the selected document context when available.
        Keep the response concise, practical, and grounded in provided inputs.
        """,
        """
        Document context: {{document_context}}
        Conversation turns: {{conversation_turn_count}}
        User question: {{user_question}}
        """,
        [
            "Answer the user question directly.",
            "Mention when document context is missing or limited.",
            "Prepare the response so citations can be attached in a later RAG step."
        ]);

    public static IReadOnlyList<PromptVariable> BuildVariables(
        string userQuestion,
        string? documentId,
        int conversationTurnCount)
    {
        var documentContext = string.IsNullOrWhiteSpace(documentId)
            ? "selected document"
            : $"document '{documentId}'";

        return
        [
            new PromptVariable("document_context", documentContext),
            new PromptVariable("conversation_turn_count", conversationTurnCount.ToString()),
            new PromptVariable("user_question", userQuestion)
        ];
    }
}
