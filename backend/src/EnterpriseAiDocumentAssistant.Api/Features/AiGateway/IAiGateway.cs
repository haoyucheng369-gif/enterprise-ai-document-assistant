using EnterpriseAiDocumentAssistant.Api.Contracts;

namespace EnterpriseAiDocumentAssistant.Api.AiGateway;

public interface IAiGateway
{
    Task<ChatModelResponse> GenerateChatResponseAsync(
        ChatModelRequest request,
        CancellationToken cancellationToken);

    Task<ToolCallDecision?> SelectToolAsync(
        ToolSelectionModelRequest request,
        CancellationToken cancellationToken);

    IEnumerable<string> BuildResponseChunks(StructuredAssistantMessage message);
}
