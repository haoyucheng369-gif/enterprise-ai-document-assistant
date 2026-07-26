using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.DocumentUpload;

namespace EnterpriseAiDocumentAssistant.Api.Services;

public sealed class ApplicationDocumentProvider : IApplicationDocumentProvider
{
    private readonly IDocumentUploadService documentUploadService;
    private readonly IWorkspaceDataProvider workspaceDataProvider;

    public ApplicationDocumentProvider(
        IWorkspaceDataProvider workspaceDataProvider,
        IDocumentUploadService documentUploadService)
    {
        this.workspaceDataProvider = workspaceDataProvider;
        this.documentUploadService = documentUploadService;
    }

    public DocumentItemResponse? FindById(string documentId)
    {
        // Skills, tools, prompt orchestration, and citation logic all use this single document lookup boundary.
        if (string.IsNullOrWhiteSpace(documentId))
        {
            return null;
        }

        // Skills, tools, and workflows read uploaded documents through the shared workspace read model.
        var workspaceDocument = workspaceDataProvider.GetWorkspace()
            .Documents
            .FirstOrDefault(candidate =>
                string.Equals(candidate.Id, documentId, StringComparison.OrdinalIgnoreCase));

        if (workspaceDocument is not null)
        {
            return workspaceDocument;
        }

        // This fallback keeps direct upload-list reads available even if workspace composition changes later.
        var uploadedDocument = documentUploadService.ListRecent()
            .FirstOrDefault(candidate =>
                string.Equals(candidate.Id, documentId, StringComparison.OrdinalIgnoreCase));

        if (uploadedDocument is null)
        {
            return null;
        }

        return new DocumentItemResponse(
            uploadedDocument.Id,
            uploadedDocument.Title,
            uploadedDocument.Type,
            uploadedDocument.UpdatedAt,
            uploadedDocument.Status,
            uploadedDocument.Sections
                .Select(section => new DocumentSectionResponse(
                    section.Label,
                    section.Title,
                    section.Body))
                .ToArray());
    }
}
