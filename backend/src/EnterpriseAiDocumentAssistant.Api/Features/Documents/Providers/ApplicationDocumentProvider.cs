using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.Documents;

namespace EnterpriseAiDocumentAssistant.Api.Services;

public sealed class ApplicationDocumentProvider : IApplicationDocumentProvider
{
    private readonly IDocumentRepository documentRepository;

    public ApplicationDocumentProvider(IDocumentRepository documentRepository)
    {
        this.documentRepository = documentRepository;
    }

    public DocumentItemResponse? FindById(string documentId)
    {
        // Skills, tools, prompt orchestration, and citation logic all use this single document lookup boundary.
        if (string.IsNullOrWhiteSpace(documentId))
        {
            return null;
        }

        // Repository performs one ACL-filtered lookup instead of scanning the latest workspace documents.
        var uploadedDocument = documentRepository.FindById(documentId);

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
                .ToArray(),
            uploadedDocument.OwnerId);
    }
}
