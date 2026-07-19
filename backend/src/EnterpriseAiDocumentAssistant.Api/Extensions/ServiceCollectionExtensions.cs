using EnterpriseAiDocumentAssistant.Api.Audit;
using EnterpriseAiDocumentAssistant.Api.AiGateway;
using EnterpriseAiDocumentAssistant.Api.ConversationMemory;
using EnterpriseAiDocumentAssistant.Api.DocumentParsing;
using EnterpriseAiDocumentAssistant.Api.DocumentUpload;
using EnterpriseAiDocumentAssistant.Api.Guardrails;
using EnterpriseAiDocumentAssistant.Api.Harness;
using EnterpriseAiDocumentAssistant.Api.Integrations.MicrosoftGraph;
using EnterpriseAiDocumentAssistant.Api.Options;
using EnterpriseAiDocumentAssistant.Api.Planner;
using EnterpriseAiDocumentAssistant.Api.PromptOrchestration;
using EnterpriseAiDocumentAssistant.Api.Services;
using EnterpriseAiDocumentAssistant.Api.Skills;
using EnterpriseAiDocumentAssistant.Api.StructuredOutput;
using EnterpriseAiDocumentAssistant.Api.ToolGateway;
using EnterpriseAiDocumentAssistant.Api.ToolGateway.Tools;
using EnterpriseAiDocumentAssistant.Api.Workflows;

namespace EnterpriseAiDocumentAssistant.Api.Extensions;

public static class ServiceCollectionExtensions
{
    // Core application services are shared by controllers, harness checks, and future orchestration.
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<ISystemClock, SystemClock>();
        services.AddSingleton<IAuditLogger, InMemoryAuditLogger>();
        services.AddSingleton<IApiStatusProvider, ApiStatusProvider>();
        services.AddSingleton<IWorkspaceDataProvider, WorkspaceDataProvider>();
        services.AddSingleton<IConversationMemoryBuilder, ConversationMemoryBuilder>();
        services.AddSingleton<IDocumentTextExtractor, DocumentTextExtractor>();
        services.AddSingleton<IDocumentChunker, SimpleDocumentChunker>();
        services.AddSingleton<IDocumentUploadService, InMemoryDocumentUploadService>();
        services.AddSingleton<IApplicationDocumentProvider, ApplicationDocumentProvider>();
        services.AddSingleton<MockAiGateway>();
        services.AddHttpClient<OpenAiGateway>();
        services.AddSingleton<IAiGateway, RoutingAiGateway>();
        services.AddSingleton<IDocumentAssistantPromptOrchestrator, DocumentAssistantPromptOrchestrator>();
        services.AddSingleton<IStructuredAssistantResponseValidator, StructuredAssistantResponseValidator>();
        services.AddSingleton<IChatGuardrailEvaluator, ChatGuardrailEvaluator>();
        services.AddSingleton<IAgentPlanner, SimpleAgentPlanner>();
        services.AddSingleton<IDocumentReviewWorkflow, DocumentReviewWorkflow>();
        services.AddSingleton<IMicrosoftGraphGateway, MockMicrosoftGraphGateway>();
        services.AddSingleton<IHarnessRunner, HarnessRunner>();

        return services;
    }

    // Tool Gateway registrations keep internal tool execution independent from HTTP or MCP entry points.
    public static IServiceCollection AddToolGateway(this IServiceCollection services)
    {
        services.AddSingleton<ITool, GetHealthStatusTool>();
        services.AddSingleton<ITool, GetDocumentMetadataTool>();
        services.AddSingleton<IToolRegistry, InMemoryToolRegistry>();
        services.AddSingleton<IToolExecutor, ToolExecutor>();

        return services;
    }

    // Skills are reusable AI capability modules that can be called by controllers, planners, or workflows.
    public static IServiceCollection AddSkills(this IServiceCollection services)
    {
        services.AddSingleton<ISummarySkill, SummarySkill>();
        services.AddSingleton<IRiskAnalysisSkill, RiskAnalysisSkill>();
        services.AddSingleton<IEmailDraftSkill, EmailDraftSkill>();
        services.AddSingleton<IClassificationSkill, ClassificationSkill>();

        return services;
    }

}
