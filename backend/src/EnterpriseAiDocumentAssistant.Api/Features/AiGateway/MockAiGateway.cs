using System.Diagnostics;
using EnterpriseAiDocumentAssistant.Api.Audit;
using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.PromptOrchestration;

namespace EnterpriseAiDocumentAssistant.Api.AiGateway;

public sealed class MockAiGateway : IAiGateway
{
    private readonly IAuditLogger auditLogger;

    public MockAiGateway(IAuditLogger auditLogger)
    {
        this.auditLogger = auditLogger;
    }

    public Task<ChatModelResponse> GenerateChatResponseAsync(
        ChatModelRequest request,
        CancellationToken cancellationToken)
    {
        // Mock gateway implements the same contract as real providers so callers do not branch on provider type.
        var stopwatch = Stopwatch.StartNew();
        var message = BuildMockStructuredResponse(request.Prompt);
        var provider = string.IsNullOrWhiteSpace(request.ProviderOverride)
            ? "Mock"
            : request.ProviderOverride;
        var response = new ChatModelResponse(
            provider,
            "mock-document-assistant",
            message,
            EstimateTokens(request.Prompt.SystemMessage) + EstimateTokens(request.Prompt.UserMessage),
            EstimateTokens(message.Answer),
            stopwatch.ElapsedMilliseconds);

        // Gateway audit records provider-facing metadata before a real model provider is introduced.
        auditLogger.Record(new AuditEventRequest(
            "ai_gateway",
            "chat_model_completed",
            provider,
            true,
            response.LatencyMs,
            new Dictionary<string, string>
            {
                ["model"] = response.Model,
                ["inputTokenEstimate"] = response.InputTokenEstimate.ToString(),
                ["outputTokenEstimate"] = response.OutputTokenEstimate.ToString()
            }));

        return Task.FromResult(response);
    }

    public IEnumerable<string> BuildResponseChunks(StructuredAssistantMessage message)
    {
        // Mock streaming uses simple chunks; real provider token streaming can replace this later.
        yield return message.Answer;
        yield return $" Confidence: {message.Confidence}.";

        if (message.SuggestedActions.Count > 0)
        {
            yield return $" Suggested action: {message.SuggestedActions[0]}";
        }
    }

    private static StructuredAssistantMessage BuildMockStructuredResponse(OrchestratedPrompt prompt)
    {
        // Mock response echoes prompt variables to make orchestration visible while debugging.
        var documentContext = GetVariable(prompt, "document_context");
        var userQuestion = GetVariable(prompt, "user_question");

        return new StructuredAssistantMessage(
            $"Using AI Gateway provider 'Mock', I am reviewing {documentContext}. Your question was: \"{userQuestion}\". The request now flows through prompt orchestration, AI Gateway, and structured output validation.",
            "medium",
            [],
            [
                "Review the AI Gateway contract.",
                "Replace MockAiGateway with an OpenAI or Azure OpenAI provider in a later step.",
                "Use citations when RAG retrieval is added."
            ]);
    }

    private static string GetVariable(OrchestratedPrompt prompt, string name)
    {
        // Mock provider depends on the same prompt variables that real providers receive.
        return prompt.Variables
            .First(variable => string.Equals(variable.Name, name, StringComparison.Ordinal))
            .Value;
    }

    private static int EstimateTokens(string value)
    {
        // Lightweight estimate used for local observability before provider usage data exists.
        return Math.Max(1, (int)Math.Ceiling(value.Length / 4.0));
    }
}
