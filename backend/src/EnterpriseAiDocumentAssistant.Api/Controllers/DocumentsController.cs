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
        var result = await documentUploadService.UploadAsync(form.File, cancellationToken);
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
}
