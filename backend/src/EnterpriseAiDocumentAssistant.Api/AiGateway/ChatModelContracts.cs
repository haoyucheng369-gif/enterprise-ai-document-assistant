using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.PromptOrchestration;

namespace EnterpriseAiDocumentAssistant.Api.AiGateway;

public sealed record ChatModelRequest(
    OrchestratedPrompt Prompt,
    string? ProviderOverride = null);

public sealed record ChatModelResponse(
    string Provider,
    string Model,
    StructuredAssistantMessage Message,
    int InputTokenEstimate,
    int OutputTokenEstimate,
    long LatencyMs);
