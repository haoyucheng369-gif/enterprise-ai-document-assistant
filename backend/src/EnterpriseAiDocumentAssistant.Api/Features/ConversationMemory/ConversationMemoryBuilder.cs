using EnterpriseAiDocumentAssistant.Api.Contracts;

namespace EnterpriseAiDocumentAssistant.Api.ConversationMemory;

public sealed class ConversationMemoryBuilder : IConversationMemoryBuilder
{
    private const int MaxTurns = 4;
    private const int MaxContentLength = 240;

    public ConversationMemoryContext Build(IReadOnlyList<MessageResponse>? history)
    {
        if (history is null || history.Count == 0)
        {
            return new ConversationMemoryContext([], "No prior conversation context.");
        }

        // Keep memory small and deterministic so it can be safely injected into the prompt.
        var turns = history
            .Where(message => !string.IsNullOrWhiteSpace(message.Content))
            .TakeLast(MaxTurns)
            .Select(message => new ConversationMemoryTurn(
                NormalizeRole(message.Role),
                Truncate(message.Content.Trim())))
            .ToArray();

        if (turns.Length == 0)
        {
            return new ConversationMemoryContext([], "No prior conversation context.");
        }

        var promptText = string.Join(
            Environment.NewLine,
            turns.Select(turn => $"{turn.Role}: {turn.Content}"));

        return new ConversationMemoryContext(turns, promptText);
    }

    private static string NormalizeRole(string role)
    {
        return role.Trim().ToLowerInvariant() switch
        {
            "assistant" => "assistant",
            "user" => "user",
            _ => "user"
        };
    }

    private static string Truncate(string content)
    {
        return content.Length <= MaxContentLength
            ? content
            : string.Concat(content.AsSpan(0, MaxContentLength), "...");
    }
}
