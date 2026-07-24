using System.Text.Json;
using EnterpriseAiDocumentAssistant.Api.AiGateway;
using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.Options;
using EnterpriseAiDocumentAssistant.Api.PromptOrchestration;
using EnterpriseAiDocumentAssistant.Api.Services;
using Microsoft.Extensions.Options;

namespace EnterpriseAiDocumentAssistant.Api.Skills;

public sealed class ResumeReviewSkill : IResumeReviewSkill
{
    private readonly IAiGateway aiGateway;
    private readonly IApplicationDocumentProvider applicationDocumentProvider;
    private readonly AiGatewayOptions options;

    public ResumeReviewSkill(
        IApplicationDocumentProvider applicationDocumentProvider,
        IAiGateway aiGateway,
        IOptions<AiGatewayOptions> options)
    {
        this.applicationDocumentProvider = applicationDocumentProvider;
        this.aiGateway = aiGateway;
        this.options = options.Value;
    }

    public async Task<ResumeReviewSkillResponse?> RunAsync(
        ResumeReviewSkillRequest request,
        CancellationToken cancellationToken)
    {
        var document = applicationDocumentProvider.FindById(request.DocumentId);
        if (document is null)
        {
            return null;
        }

        var provider = SkillJsonReader.ResolveProvider(options, request.AiProvider);
        if (SkillJsonReader.IsMockProvider(provider))
        {
            // Mock returns a readable Markdown review brief without spending model tokens.
            return BuildDeterministicReview(document, provider);
        }

        // Real AI path: prompt template receives parsed resume text and controls the output language.
        var prompt = DocumentSkillPromptTemplates.BuildResumeReviewPrompt(
            document,
            request.Instruction);
        var modelResponse = await aiGateway.GenerateChatResponseAsync(
            new ChatModelRequest(prompt, provider),
            cancellationToken);

        // The model still returns JSON; the Markdown document lives inside the content property.
        return TryParseModelReview(document.Id, provider, modelResponse.Message.Answer)
            ?? BuildDeterministicReview(document, provider);
    }

    private static ResumeReviewSkillResponse BuildDeterministicReview(
        DocumentItemResponse document,
        string provider)
    {
        var content = $"""
        # Resume Review Brief

        ## Source document

        {document.Title}

        ## Strengths

        {string.Join(Environment.NewLine, document.Sections.Take(6).Select(section => $"- **{section.Title}**: {section.Body}"))}

        ## Items to review

        - Use OpenAI or Azure OpenAI provider to generate a fuller review from the parsed resume content.

        ## Rewrite instructions for ChatGPT

        - Combine this review brief with the original resume.
        - Keep real experience and technical facts, then improve structure, positioning, and keyword coverage.
        """;

        return new ResumeReviewSkillResponse(
            document.Id,
            $"{document.Title} - Resume Review Brief",
            "markdown",
            content,
            document.Sections.Select(section => section.Label).Take(6).ToArray(),
            ["Generate full review with OpenAI", "Check missing keywords", "Use with the original resume in ChatGPT"],
            provider);
    }

    private static ResumeReviewSkillResponse? TryParseModelReview(
        string documentId,
        string provider,
        string answer)
    {
        if (!answer.TrimStart().StartsWith('{'))
        {
            return null;
        }

        try
        {
            using var jsonDocument = JsonDocument.Parse(answer);
            var root = jsonDocument.RootElement;

            return new ResumeReviewSkillResponse(
                documentId,
                SkillJsonReader.ReadString(root, "title", "Resume Review Brief"),
                SkillJsonReader.ReadString(root, "format", "markdown"),
                SkillJsonReader.ReadString(root, "content", string.Empty),
                SkillJsonReader.ReadStringArray(root, "basedOn"),
                SkillJsonReader.ReadStringArray(root, "nextActions"),
                provider);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
