using EnterpriseAiDocumentAssistant.Api.RateLimiting;
using EnterpriseAiDocumentAssistant.Api.Workflows;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiDocumentAssistant.Api.Controllers;

[ApiController]
[Route("api/workflows")]
[EnableRateLimiting(AiRateLimitPolicy.Name)]
public sealed class WorkflowsController : ControllerBase
{
    private readonly IDocumentReviewWorkflow documentReviewWorkflow;

    public WorkflowsController(IDocumentReviewWorkflow documentReviewWorkflow)
    {
        this.documentReviewWorkflow = documentReviewWorkflow;
    }

    [HttpPost("document-review")]
    [ProducesResponseType<DocumentReviewWorkflowResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentReviewWorkflowResponse>> RunDocumentReview(
        DocumentReviewWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        // Agent Handoff Step 1: receive the HTTP request and validate it before entering the workflow.
        // Debug next: DocumentReviewWorkflow.RunAsync -> DocumentAgent -> EmailAgent -> final response.
        if (string.IsNullOrWhiteSpace(request.DocumentId))
        {
            ModelState.AddModelError(nameof(request.DocumentId), "DocumentId is required.");
            return ValidationProblem(ModelState);
        }

        var result = await documentReviewWorkflow.RunAsync(request, cancellationToken);
        if (result is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "WorkflowInputNotFound",
                Detail = $"Document '{request.DocumentId}' was not found or could not be processed."
            });
        }

        return Ok(result);
    }
}
