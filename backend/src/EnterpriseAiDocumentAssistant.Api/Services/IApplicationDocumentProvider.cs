using EnterpriseAiDocumentAssistant.Api.Contracts;

namespace EnterpriseAiDocumentAssistant.Api.Services;

public interface IApplicationDocumentProvider
{
    DocumentItemResponse? FindById(string documentId);
}
