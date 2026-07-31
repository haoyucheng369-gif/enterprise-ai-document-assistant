using System.Text.Json;
using EnterpriseAiDocumentAssistant.Api.AiGateway;
using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.PromptOrchestration;
using EnterpriseAiDocumentAssistant.Api.ToolGateway;

namespace EnterpriseAiDocumentAssistant.Api.ToolCalling;

public sealed class SingleTurnToolCallingService : IToolCallingService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IAiGateway aiGateway;
    private readonly IToolExecutor toolExecutor;
    private readonly IToolRegistry toolRegistry;

    public SingleTurnToolCallingService(
        IAiGateway aiGateway,
        IToolExecutor toolExecutor,
        IToolRegistry toolRegistry)
    {
        this.aiGateway = aiGateway;
        this.toolExecutor = toolExecutor;
        this.toolRegistry = toolRegistry;
    }

    public async Task<StructuredAssistantMessage?> ExecuteSingleToolCallAsync(
        ChatRequest request,
        CancellationToken cancellationToken)
    {
        // Step 1: expose only read-only tools and let the selected model choose at most one.
        var tools = toolRegistry.ListDefinitions()
            .Where(tool => tool.IsReadOnly)
            .ToArray();
        var decision = await aiGateway.SelectToolAsync(
            new ToolSelectionModelRequest(
                request.Message,
                request.DocumentId,
                tools,
                request.AiProvider),
            cancellationToken);

        if (decision is null)
        {
            return null;
        }

        // Step 2: execute through Tool Gateway so lookup, validation, errors, and audit stay centralized.
        var arguments = AddSelectedDocumentIdWhenMissing(decision, request.DocumentId);
        var toolResult = await toolExecutor.ExecuteAsync(
            new ToolExecutionRequest(decision.ToolName, arguments),
            cancellationToken);

        // Step 3: return the tool result to the model so it can produce the user-facing answer.
        var prompt = BuildToolResultPrompt(request, toolResult);
        var modelResponse = await aiGateway.GenerateChatResponseAsync(
            new ChatModelRequest(prompt, request.AiProvider),
            cancellationToken);

        return modelResponse.Message;
    }

    private static IReadOnlyDictionary<string, JsonElement> AddSelectedDocumentIdWhenMissing(
        ToolCallDecision decision,
        string? documentId)
    {
        if (!string.Equals(
                decision.ToolName,
                "get_document_metadata",
                StringComparison.OrdinalIgnoreCase)
            || decision.Arguments.ContainsKey("documentId")
            || string.IsNullOrWhiteSpace(documentId))
        {
            return decision.Arguments;
        }

        var arguments = new Dictionary<string, JsonElement>(decision.Arguments)
        {
            ["documentId"] = JsonSerializer.SerializeToElement(documentId)
        };

        return arguments;
    }

    private static OrchestratedPrompt BuildToolResultPrompt(
        ChatRequest request,
        ToolExecutionResult toolResult)
    {
        var serializedResult = JsonSerializer.Serialize(toolResult, JsonOptions);
        var variables = new[]
        {
            new PromptVariable("document_context", serializedResult),
            new PromptVariable("user_question", request.Message),
            new PromptVariable("tool_name", toolResult.ToolName),
            new PromptVariable("tool_result", serializedResult)
        };

        return new OrchestratedPrompt(
            "tool-result-response-v1",
            EnterpriseAssistantPromptDefaults.BuildSystemMessage(
                "Answer the user using only the result returned by the executed read-only tool."),
            $"""
             User request:
             {request.Message}

             Executed tool:
             {toolResult.ToolName}

             Tool result:
             {serializedResult}
             """,
            EnterpriseAssistantPromptDefaults.CombineOutputRules(
                EnterpriseAssistantPromptDefaults.OutputRules,
                [
                    "Explain a failed tool result clearly without inventing missing data.",
                    "Do not claim that another tool was executed.",
                    "Keep the answer concise."
                ]),
            variables);
    }
}
