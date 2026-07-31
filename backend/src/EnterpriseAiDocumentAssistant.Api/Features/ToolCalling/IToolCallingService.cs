using EnterpriseAiDocumentAssistant.Api.Contracts;

namespace EnterpriseAiDocumentAssistant.Api.ToolCalling;

public interface IToolCallingService
{
    Task<StructuredAssistantMessage?> ExecuteSingleToolCallAsync(
        ChatRequest request,
        CancellationToken cancellationToken);
}
