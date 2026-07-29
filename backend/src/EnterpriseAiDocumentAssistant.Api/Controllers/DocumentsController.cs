using EnterpriseAiDocumentAssistant.Api.DocumentUpload;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiDocumentAssistant.Api.Controllers;

[ApiController]
[Route("api/documents")]
public sealed class DocumentsController : ControllerBase
{
    private readonly IDocumentUploadService documentUploadService;

    public DocumentsController(IDocumentUploadService documentUploadService)
    {
        this.documentUploadService = documentUploadService;
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
        var result = await documentUploadService.UploadAsync(form.File, form.AiProvider, cancellationToken);
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
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        string documentId,
        CancellationToken cancellationToken)
    {
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
