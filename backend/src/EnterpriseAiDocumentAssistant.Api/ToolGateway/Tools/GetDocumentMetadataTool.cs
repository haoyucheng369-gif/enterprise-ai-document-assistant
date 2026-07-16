using System.Text.Json;
using EnterpriseAiDocumentAssistant.Api.Services;

namespace EnterpriseAiDocumentAssistant.Api.ToolGateway.Tools;

public sealed class GetDocumentMetadataTool : ITool
{
    private readonly IWorkspaceDataProvider workspaceDataProvider;

    public GetDocumentMetadataTool(IWorkspaceDataProvider workspaceDataProvider)
    {
        this.workspaceDataProvider = workspaceDataProvider;
    }

    public ToolDefinition Definition { get; } = new(
        "get_document_metadata",
        "Returns metadata for a document in the current workspace.",
        new Dictionary<string, ToolParameterDefinition>
        {
            ["documentId"] = new ToolParameterDefinition(
                "string",
                "Document id from the workspace document list.",
                true)
        });

    public Task<ToolExecutionResult> ExecuteAsync(
        ToolExecutionRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetDocumentId(request.Arguments, out var documentId))
        {
            return Task.FromResult(new ToolExecutionResult(
                Definition.Name,
                false,
                "Argument 'documentId' is required.",
                new Dictionary<string, object?>()));
        }

        var document = workspaceDataProvider.GetWorkspace()
            .Documents
            .FirstOrDefault(candidate =>
                string.Equals(candidate.Id, documentId, StringComparison.OrdinalIgnoreCase));

        if (document is null)
        {
            return Task.FromResult(new ToolExecutionResult(
                Definition.Name,
                false,
                $"Document '{documentId}' was not found.",
                new Dictionary<string, object?>()));
        }

        return Task.FromResult(new ToolExecutionResult(
            Definition.Name,
            true,
            null,
            new Dictionary<string, object?>
            {
                ["documentId"] = document.Id,
                ["title"] = document.Title,
                ["type"] = document.Type,
                ["updatedAt"] = document.UpdatedAt,
                ["status"] = document.Status,
                ["sectionCount"] = document.Sections.Count,
                ["sections"] = document.Sections.Select(section => new
                {
                    section.Label,
                    section.Title
                }).ToArray()
            }));
    }

    private static bool TryGetDocumentId(
        IReadOnlyDictionary<string, JsonElement> arguments,
        out string documentId)
    {
        documentId = string.Empty;

        if (!arguments.TryGetValue("documentId", out var documentIdElement))
        {
            return false;
        }

        if (documentIdElement.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        documentId = documentIdElement.GetString()?.Trim() ?? string.Empty;
        return documentId.Length > 0;
    }
}
