using EnterpriseAiDocumentAssistant.Api.DocumentUpload;
using EnterpriseAiDocumentAssistant.Api.Security;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiDocumentAssistant.Api.Controllers;

[ApiController]
[Route("api/documents")]
public sealed class DocumentsController : ControllerBase
{
    private readonly IDocumentUploadService documentUploadService;
    private readonly IDocumentAccessPolicy documentAccessPolicy;

    public DocumentsController(
        IDocumentUploadService documentUploadService,
        IDocumentAccessPolicy documentAccessPolicy)
    {
        this.documentUploadService = documentUploadService;
        this.documentAccessPolicy = documentAccessPolicy;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<DocumentUploadResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DocumentUploadResponse>> Upload(
        [FromForm] DocumentUploadForm form,
        CancellationToken cancellationToken)
    {
        // HTTP upload endpoint delegates validation, parsing, chunking, persistence, and audit to the upload service.
        // Upload uses the same provider selected by the workspace so parsing and RAG indexing follow one global mode.
        var allowedUserIds = form.AllowedUserIds?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var result = await documentUploadService.UploadAsync(
            form.File,
            form.AiProvider,
            allowedUserIds,
            cancellationToken);
        if (!result.Succeeded || result.Document is null)
        {
            ModelState.AddModelError(nameof(form.File), result.Error ?? "Upload failed.");
            return ValidationProblem(ModelState);
        }

        return Ok(result.Document);
    }

    [HttpGet("uploads")]
    [ProducesResponseType<IReadOnlyList<DocumentUploadResponse>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<DocumentUploadResponse>> ListUploads()
    {
        // Quick backend check endpoint for uploaded documents stored in MongoDB.
        return Ok(documentUploadService.ListRecent());
    }

    [HttpDelete("{documentId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        string documentId,
        CancellationToken cancellationToken)
    {
        var access = documentAccessPolicy.Evaluate(documentId);
        if (access is DocumentAccessLevel.Denied or DocumentAccessLevel.Reader)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Title = "DocumentAccessDenied",
                Detail = "Only the document owner can delete this document.",
                Status = StatusCodes.Status403Forbidden
            });
        }

        // Delete endpoint keeps document removal behind the same service that owns upload persistence and audit.
        var deleted = await documentUploadService.DeleteAsync(documentId, cancellationToken);
        if (!deleted)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Document not found.",
                Detail = $"No uploaded document exists with id '{documentId}'.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return NoContent();
    }
}
