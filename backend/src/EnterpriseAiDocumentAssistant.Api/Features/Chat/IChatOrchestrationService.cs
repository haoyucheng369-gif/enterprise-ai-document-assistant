using EnterpriseAiDocumentAssistant.Api.Contracts;

namespace EnterpriseAiDocumentAssistant.Api.Chat;

public interface IChatOrchestrationService
{
    Task<ChatOrchestrationResult> BuildValidatedMessageAsync(
        ChatRequest request,
        CancellationToken cancellationToken);
}

public sealed record ChatOrchestrationResult(
    StructuredAssistantMessage? Message,
    IReadOnlyList<string> Errors)
{
    public bool IsValid => Message is not null && Errors.Count == 0;
}
