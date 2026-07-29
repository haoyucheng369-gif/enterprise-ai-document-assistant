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
        // The model sees the user's message plus the allowed backend routes, but it does not execute anything.
        var routes = string.Join(Environment.NewLine, AgentPlanCatalog.Routes.Select(route => $"- {route}"));
        const string factQuestionExample =
            """{"intent":"document_question","route":"chat","reason":"The user asks for a fact that should be retrieved from the selected document."}""";
        const string summaryExample =
            """{"intent":"summary","route":"skills.summary","reason":"The user explicitly asks for a summary of the entire document."}""";
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

            Routing policy:
            - chat is the default route for document questions and must use RAG.
            - Use chat for facts, dates, durations, comparisons, calculations, explanations, and questions about one part of a document.
            - Merely mentioning a resume, CV, risk, email, category, or summary does not justify a specialized route.
            - Select a skill only when the user explicitly asks the application to perform that complete specialized action.
            - skills.summary means summarizing the entire document, not answering a focused question.
            - skills.resume-review means producing a complete resume assessment, not answering a fact about a candidate.
            - Use workflows.document-review only when the user explicitly requests the complete multi-step workflow.
            - When uncertain between chat and a skill, choose chat.

            Return the chosen route as compact JSON in the answer field only.
            Fact-question example:
            User: Across the two positions, how many years did the candidate work?
            Answer: {factQuestionExample}

            Full-summary example:
            User: Summarize the entire selected document.
            Answer: {summaryExample}
            """,
            EnterpriseAssistantPromptDefaults.CombineOutputRules(
                EnterpriseAssistantPromptDefaults.OutputRules,
                [
                    "The answer field must contain compact JSON only.",
                    "The JSON must include intent, route, and reason.",
                    "The route must be one of the allowed routes.",
                    "Use chat unless the user clearly requests a complete specialized action."
                ]),
            variables);
    }

    private static AgentRouteDecision? TryParseDecision(string answer)
    {
        // Invalid or non-JSON model decisions are treated as no decision so fallback routing can run.
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
