namespace EnterpriseAiDocumentAssistant.Api.Contracts;

public sealed record WorkspaceResponse(
    IReadOnlyList<DocumentItemResponse> Documents,
    IReadOnlyList<MessageResponse> Messages,
    IReadOnlyList<CitationResponse> Citations,
    ToolResultResponse ToolResult);

public sealed record DocumentItemResponse(
    string Id,
    string Title,
    string Type,
    string UpdatedAt,
    string Status,
    IReadOnlyList<DocumentSectionResponse> Sections,
    string OwnerId);

public sealed record DocumentSectionResponse(
    string Label,
    string Title,
    string Body);

public sealed record MessageResponse(
    string Id,
    string Role,
    string Content,
    string? Confidence = null,
    IReadOnlyList<string>? Citations = null,
    IReadOnlyList<string>? SuggestedActions = null);

public sealed record CitationResponse(
    string Id,
    string Label);

public sealed record ToolResultResponse(
    string Name,
    string Status,
    string Description);
