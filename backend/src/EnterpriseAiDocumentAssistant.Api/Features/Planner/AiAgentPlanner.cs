using System.Text.Json;
using EnterpriseAiDocumentAssistant.Api.AiGateway;
using EnterpriseAiDocumentAssistant.Api.PromptOrchestration;

namespace EnterpriseAiDocumentAssistant.Api.Planner;

public sealed class AiAgentPlanner
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IAiGateway aiGateway;

    public AiAgentPlanner(IAiGateway aiGateway)
    {
        this.aiGateway = aiGateway;
    }

    public async Task<AgentPlanResponse?> TryPlanAsync(
        AgentPlanRequest request,
        CancellationToken cancellationToken)
    {
        // Ask the model to choose a route only; backend code still owns execution and validation.
        var prompt = BuildRouteSelectionPrompt(request);
        var modelResponse = await aiGateway.GenerateChatResponseAsync(
            new ChatModelRequest(prompt, request.AiProvider),
            cancellationToken);

        // The model's answer field should be compact JSON containing the selected route.
        var decision = TryParseDecision(modelResponse.Message.Answer);
        if (decision is null || !AgentPlanCatalog.IsKnownRoute(decision.Route))
        {
            return null;
        }

        return AgentPlanCatalog.Create(decision.Route, request.DocumentId);
    }

    private static OrchestratedPrompt BuildRouteSelectionPrompt(AgentPlanRequest request)
    {
        var routes = string.Join(Environment.NewLine, AgentPlanCatalog.Routes.Select(route => $"- {route}"));
        const string exampleAnswer = """{"intent":"summary","route":"skills.summary","reason":"The user asks for a document summary."}""";
        var variables = new[]
        {
            new PromptVariable("user_message", request.Message),
            new PromptVariable("document_id", request.DocumentId ?? string.Empty),
            new PromptVariable("available_routes", routes)
        };

        // The AI planner chooses only a route; backend code still executes the selected skill or workflow.
        return new OrchestratedPrompt(
            "agent-intent-router-v1",
            EnterpriseAssistantPromptDefaults.BuildSystemMessage(
                "Classify the user's intent and choose exactly one route from the allowed route list."),
            $"""
            User message:
            {request.Message}

            Selected document id:
            {request.DocumentId ?? "No document selected"}

            Allowed routes:
            {routes}

            Return the chosen route as compact JSON in the answer field only.
            Example answer:
            {exampleAnswer}
            """,
            EnterpriseAssistantPromptDefaults.CombineOutputRules(
                EnterpriseAssistantPromptDefaults.OutputRules,
                [
                    "The answer field must contain compact JSON only.",
                    "The JSON must include intent, route, and reason.",
                    "The route must be one of the allowed routes.",
                    "Use chat when no specialized route is clearly required."
                ]),
            variables);
    }

    private static AgentRouteDecision? TryParseDecision(string answer)
    {
        try
        {
            return JsonSerializer.Deserialize<AgentRouteDecision>(answer, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed record AgentRouteDecision(
        string Intent,
        string Route,
        string Reason);
}
