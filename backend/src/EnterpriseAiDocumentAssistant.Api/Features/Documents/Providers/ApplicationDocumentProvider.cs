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
        if (string.IsNullOrWhiteSpace(documentId))
        {
            return null;
        }

        // Seed documents and uploaded documents share one read model for skills, tools, and workflows.
        var workspaceDocument = workspaceDataProvider.GetWorkspace()
            .Documents
            .FirstOrDefault(candidate =>
                string.Equals(candidate.Id, documentId, StringComparison.OrdinalIgnoreCase));

        if (workspaceDocument is not null)
        {
            return workspaceDocument;
        }

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
