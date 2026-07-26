using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.Documents;

namespace EnterpriseAiDocumentAssistant.Api.Services;

public sealed class WorkspaceDataProvider : IWorkspaceDataProvider
{
    private readonly IDocumentRepository documentRepository;

    public WorkspaceDataProvider(IDocumentRepository documentRepository)
    {
        this.documentRepository = documentRepository;
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

        return new WorkspaceResponse(
            // The workspace now reflects only persisted user documents; no hardcoded sample documents are merged in.
            Documents: persistedDocuments,
            Messages:
            [
                new MessageResponse(
                    "m1",
                    "assistant",
                    "Select a document and ask a question. I can summarize, classify, review risks, and suggest follow-up actions.")
            ],
            Citations: [],
            ToolResult: new ToolResultResponse(
                "GetDocumentMetadataTool",
                "Ready",
                "Select an uploaded document to inspect metadata."));
    }
}
