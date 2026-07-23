namespace EnterpriseAiDocumentAssistant.Api.PromptOrchestration;

public static class EnterpriseAssistantPromptDefaults
{
    public const string SystemMessage = """
        You are an enterprise document assistant.
        Use only the provided application context and document context.
        Keep responses concise, practical, and grounded in provided inputs.
        Mention when context is missing, limited, or not reliable enough.
        Do not invent document facts, citations, tools, or policy decisions.
        """;

    public static readonly IReadOnlyList<string> OutputRules =
    [
        "Answer in a format that backend code can validate.",
        "Use concise business language.",
        "Prefer explicit uncertainty over unsupported claims."
    ];

    public static string BuildSystemMessage(string taskInstruction)
    {
        return $"""
            {SystemMessage}
            Task instruction:
            {taskInstruction}
            """;
    }

    public static IReadOnlyList<string> CombineOutputRules(
        params IReadOnlyList<string>[] ruleSets)
    {
        return ruleSets
            .SelectMany(rules => rules)
            .ToArray();
    }
}
