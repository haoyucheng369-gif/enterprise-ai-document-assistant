using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.Options;
using Microsoft.Extensions.Options;

namespace EnterpriseAiDocumentAssistant.Api.AiGateway;

public sealed class RoutingAiGateway : IAiGateway
{
    private readonly MockAiGateway mockAiGateway;
    private readonly OpenAiGateway openAiGateway;
    private readonly AiGatewayOptions options;

    public RoutingAiGateway(
        MockAiGateway mockAiGateway,
        OpenAiGateway openAiGateway,
        IOptions<AiGatewayOptions> options)
    {
        this.mockAiGateway = mockAiGateway;
        this.openAiGateway = openAiGateway;
        this.options = options.Value;
    }

    public Task<ChatModelResponse> GenerateChatResponseAsync(
        ChatModelRequest request,
        CancellationToken cancellationToken)
    {
        var provider = ResolveProvider(request.ProviderOverride);
        var routedRequest = request with { ProviderOverride = provider };

        return IsRealProvider(provider)
            ? openAiGateway.GenerateChatResponseAsync(routedRequest, cancellationToken)
            : mockAiGateway.GenerateChatResponseAsync(routedRequest, cancellationToken);
    }

    public IEnumerable<string> BuildResponseChunks(StructuredAssistantMessage message)
    {
        yield return message.Answer;
        yield return $" Confidence: {message.Confidence}.";

        if (message.SuggestedActions.Count > 0)
        {
            yield return $" Suggested action: {message.SuggestedActions[0]}";
        }
    }

    private string ResolveProvider(string? requestedProvider)
    {
        return string.IsNullOrWhiteSpace(requestedProvider)
            ? options.Provider
            : requestedProvider.Trim();
    }

    private static bool IsRealProvider(string provider)
    {
        return string.Equals(provider, "OpenAI", StringComparison.OrdinalIgnoreCase)
            || string.Equals(provider, "AzureOpenAI", StringComparison.OrdinalIgnoreCase);
    }
}
