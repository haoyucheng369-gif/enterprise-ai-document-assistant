using EnterpriseAiDocumentAssistant.Api.Contracts;

namespace EnterpriseAiDocumentAssistant.Api.Services;

public interface IWorkspaceDataProvider
{
    WorkspaceResponse GetWorkspace();
}
