using System.Text.Json;
using EnterpriseAiDocumentAssistant.Api.AiGateway;
using EnterpriseAiDocumentAssistant.Api.Planner;
using EnterpriseAiDocumentAssistant.Api.PromptOrchestration;

namespace EnterpriseAiDocumentAssistant.Api.IntentClassification;

public sealed class AiIntentClassifier
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IAiGateway aiGateway;

    public AiIntentClassifier(IAiGateway aiGateway)
    {
        this.aiGateway = aiGateway;
    }

    public async Task<IntentClassificationResult?> TryClassifyAsync(
        IntentClassificationRequest request,
        CancellationToken cancellationToken)
    {
        // The model classifies intent only; it does not choose implementation classes or execute capabilities.
        var modelResponse = await aiGateway.GenerateChatResponseAsync(
            new ChatModelRequest(BuildIntentPrompt(request), request.AiProvider),
            cancellationToken);
        var decision = TryParseDecision(modelResponse.Message.Answer);

        return decision is not null && AgentPlanCatalog.IsKnownIntent(decision.Intent)
            ? new IntentClassificationResult(decision.Intent, decision.Reason, "ai")
            : null;
    }

    private static OrchestratedPrompt BuildIntentPrompt(IntentClassificationRequest request)
    {
        var intents = string.Join(
            Environment.NewLine,
            AgentPlanCatalog.Intents.Select(intent => $"- {intent}"));
        var variables = new[]
        {
            new PromptVariable("user_message", request.Message),
            new PromptVariable("document_id", request.DocumentId ?? string.Empty),
            new PromptVariable("available_intents", intents)
        };
        const string example =
            """{"intent":"document_question","reason":"The user asks for one fact from the document."}""";

        return new OrchestratedPrompt(
            "agent-intent-classifier-v1",
            EnterpriseAssistantPromptDefaults.BuildSystemMessage(
                "Classify the user's request into exactly one allowed application intent."),
            $"""
             User message:
             {request.Message}

             Selected document id:
             {request.DocumentId ?? "No document selected"}

             Allowed intents:
             {intents}

             Classification policy:
             - document_question is the default for facts, comparisons, calculations, explanations, and focused questions.
             - Select a specialized intent only for an explicit complete action.
             - summary means summarizing the entire document.
             - resume_review means producing a complete resume assessment.
             - tool_request means requesting system health or document metadata.
             - document_review_workflow means explicitly requesting the complete multi-step review.

             Return compact JSON in the answer field only.
             Example: {example}
             """,
            EnterpriseAssistantPromptDefaults.CombineOutputRules(
                EnterpriseAssistantPromptDefaults.OutputRules,
                [
                    "The answer field must contain compact JSON only.",
                    "The JSON must include intent and reason.",
                    "The intent must be one of the allowed intents."
                ]),
            variables);
    }

    private static IntentDecision? TryParseDecision(string answer)
    {
        try
        {
            return JsonSerializer.Deserialize<IntentDecision>(answer, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed record IntentDecision(string Intent, string Reason);
}
