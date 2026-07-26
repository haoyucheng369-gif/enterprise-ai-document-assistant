using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiDocumentAssistant.Api.Controllers;

[ApiController]
[Route("api/workspace")]
public sealed class WorkspaceController : ControllerBase
{
    private readonly IWorkspaceDataProvider workspaceDataProvider;

    public WorkspaceController(IWorkspaceDataProvider workspaceDataProvider)
    {
        this.workspaceDataProvider = workspaceDataProvider;
    }

    [HttpGet]
    [ProducesResponseType<WorkspaceResponse>(StatusCodes.Status200OK)]
    public ActionResult<WorkspaceResponse> Get()
    {
        // Frontend bootstraps the whole workspace from this single read model.
        return Ok(workspaceDataProvider.GetWorkspace());
    }
}
