using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.Conversations;
using EnterpriseAiDocumentAssistant.Api.Documents;

namespace EnterpriseAiDocumentAssistant.Api.Services;

public sealed class WorkspaceDataProvider : IWorkspaceDataProvider
{
    private readonly IDocumentRepository documentRepository;
    private readonly IConversationRepository conversationRepository;

    public WorkspaceDataProvider(
        IDocumentRepository documentRepository,
        IConversationRepository conversationRepository)
    {
        this.documentRepository = documentRepository;
        this.conversationRepository = conversationRepository;
    }

    public WorkspaceResponse GetWorkspace()
    {
        // Read uploaded documents from MongoDB first so persisted user documents appear at the top of the workspace.
        var persistedDocuments = documentRepository
            .ListRecent(20)
            .Select(document => new DocumentItemResponse(
                document.Id,
                document.Title,
                document.Type,
                document.UpdatedAt,
                document.Status,
                document.Sections.Select(section => new DocumentSectionResponse(
                    section.Label,
                    section.Title,
                    section.Body)).ToArray()))
            .ToArray();

        // Restore recent validated turns so a browser refresh does not erase the conversation.
        var messages = conversationRepository.ListRecent(25);
        var latestCitations = messages
            .LastOrDefault(message => message.Role == "assistant")?
            .Citations?
            .Select((citation, index) => new CitationResponse(
                $"persisted-citation-{index + 1}",
                citation))
            .ToArray() ?? [];

        return new WorkspaceResponse(
            // The workspace now reflects only persisted user documents; no hardcoded sample documents are merged in.
            Documents: persistedDocuments,
            Messages: messages.Count > 0
                ? messages
                :
                [
                    new MessageResponse(
                        "m1",
                        "assistant",
                        "Select a document and ask a question. I can summarize, classify, review risks, and suggest follow-up actions.")
                ],
            Citations: latestCitations,
            ToolResult: new ToolResultResponse(
                "GetDocumentMetadataTool",
                "Ready",
                "Select an uploaded document to inspect metadata."));
    }
}
