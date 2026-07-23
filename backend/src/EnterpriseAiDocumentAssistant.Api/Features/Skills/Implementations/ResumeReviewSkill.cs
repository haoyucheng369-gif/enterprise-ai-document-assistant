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

        var prompt = DocumentSkillPromptTemplates.BuildResumeReviewPrompt(
            document,
            request.Instruction);
        var modelResponse = await aiGateway.GenerateChatResponseAsync(
            new ChatModelRequest(prompt, provider),
            cancellationToken);

        // Real model output is parsed into a review contract that the frontend can display as Markdown text.
        return TryParseModelReview(document.Id, provider, modelResponse.Message.Answer)
            ?? BuildDeterministicReview(document, provider);
    }

    private static ResumeReviewSkillResponse BuildDeterministicReview(
        DocumentItemResponse document,
        string provider)
    {
        var content = $"""
        # Resume Review Brief

        ## 原始文档

        {document.Title}

        ## 优点

        {string.Join(Environment.NewLine, document.Sections.Take(6).Select(section => $"- **{section.Title}**: {section.Body}"))}

        ## 需要重点检查的问题

        - 使用 OpenAI 或 Azure OpenAI provider 可以基于解析后的简历内容生成更完整的中文评审。

        ## 给 ChatGPT 的改写指令

        - 请结合这份 Review Brief 和原始简历一起改写。
        - 保留真实经历和技术事实，重点优化结构、定位和关键词覆盖。
        """;

        return new ResumeReviewSkillResponse(
            document.Id,
            $"{document.Title} - 简历评审 Brief",
            "markdown",
            content,
            document.Sections.Select(section => section.Label).Take(6).ToArray(),
            ["使用 OpenAI 生成完整评审", "检查缺失关键词", "结合原始简历一起交给 ChatGPT"],
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
                SkillJsonReader.ReadString(root, "title", "简历评审 Brief"),
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
