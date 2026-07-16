using EnterpriseAiDocumentAssistant.Api.Contracts;

namespace EnterpriseAiDocumentAssistant.Api.Services;

public interface IApiStatusProvider
{
    StatusResponse GetStatus();
}
