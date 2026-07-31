using System.Text.Json;
using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.PromptOrchestration;
using EnterpriseAiDocumentAssistant.Api.ToolGateway;

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

public sealed record ToolSelectionModelRequest(
    string UserMessage,
    string? DocumentId,
    IReadOnlyList<ToolDefinition> Tools,
    string? ProviderOverride = null);

public sealed record ToolCallDecision(
    string ToolName,
    IReadOnlyDictionary<string, JsonElement> Arguments);
