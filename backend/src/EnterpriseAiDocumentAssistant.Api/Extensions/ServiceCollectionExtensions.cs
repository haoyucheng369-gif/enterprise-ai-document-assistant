using EnterpriseAiDocumentAssistant.Api.Audit;
using EnterpriseAiDocumentAssistant.Api.Agents;
using EnterpriseAiDocumentAssistant.Api.AiGateway;
using EnterpriseAiDocumentAssistant.Api.Chat;
using EnterpriseAiDocumentAssistant.Api.ConversationMemory;
using EnterpriseAiDocumentAssistant.Api.Conversations;
using EnterpriseAiDocumentAssistant.Api.DocumentParsing;
using EnterpriseAiDocumentAssistant.Api.DocumentUpload;
using EnterpriseAiDocumentAssistant.Api.Documents;
using EnterpriseAiDocumentAssistant.Api.Guardrails;
using EnterpriseAiDocumentAssistant.Api.Harness;
using EnterpriseAiDocumentAssistant.Api.Integrations.MicrosoftGraph;
using EnterpriseAiDocumentAssistant.Api.IntentClassification;
using EnterpriseAiDocumentAssistant.Api.Options;
using EnterpriseAiDocumentAssistant.Api.Planner;
using EnterpriseAiDocumentAssistant.Api.PromptOrchestration;
using EnterpriseAiDocumentAssistant.Api.Rag;
using EnterpriseAiDocumentAssistant.Api.Security;
using EnterpriseAiDocumentAssistant.Api.Services;
using EnterpriseAiDocumentAssistant.Api.Skills;
using EnterpriseAiDocumentAssistant.Api.StructuredOutput;
using EnterpriseAiDocumentAssistant.Api.ToolGateway;
using EnterpriseAiDocumentAssistant.Api.ToolGateway.Tools;
using EnterpriseAiDocumentAssistant.Api.ToolCalling;
using EnterpriseAiDocumentAssistant.Api.Workflows;
using Microsoft.Extensions.Options;

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
        services.AddHttpContextAccessor();
        services.AddSingleton<ICurrentUserAccessor, HttpHeaderCurrentUserAccessor>();
        services.AddSingleton<IDocumentTextExtractor, DocumentTextExtractor>();
        services.AddSingleton<IDocumentChunker, SimpleDocumentChunker>();

        // Repository hides MongoDB from upload, workspace, skills, and tools.
        services.AddSingleton<MongoDocumentRepository>();
        services.AddSingleton<IDocumentRepository>(serviceProvider =>
            serviceProvider.GetRequiredService<MongoDocumentRepository>());
        services.AddSingleton<IDocumentAccessPolicy>(serviceProvider =>
            serviceProvider.GetRequiredService<MongoDocumentRepository>());
        services.AddSingleton<IConversationRepository, MongoConversationRepository>();
        services.AddSingleton<IDocumentUploadService, DocumentUploadService>();
        services.AddSingleton<IApplicationDocumentProvider, ApplicationDocumentProvider>();
        services.AddSingleton<MockAiGateway>();
        services.AddHttpClient<OpenAiGateway>();
        services.AddSingleton<IAiGateway, RoutingAiGateway>();
        services.AddHttpClient<RoutingEmbeddingGateway>();
        services.AddSingleton<IEmbeddingGateway, RoutingEmbeddingGateway>();
        services.AddSingleton<InMemoryVectorStore>();
        services.AddSingleton<QdrantVectorStore>();
        services.AddSingleton<IVectorStore>(serviceProvider =>
        {
            var ragOptions = serviceProvider.GetRequiredService<IOptions<RagOptions>>().Value;
            return string.Equals(ragOptions.VectorStore, "Qdrant", StringComparison.OrdinalIgnoreCase)
                ? serviceProvider.GetRequiredService<QdrantVectorStore>()
                : serviceProvider.GetRequiredService<InMemoryVectorStore>();
        });
        services.AddSingleton<IRagService, RagService>();
        services.AddSingleton<IDocumentAssistantPromptOrchestrator, DocumentAssistantPromptOrchestrator>();
        services.AddSingleton<IPlannedCapabilityExecutor, PlannedCapabilityExecutor>();
        services.AddSingleton<IChatOrchestrationService, ChatOrchestrationService>();
        services.AddSingleton<IStructuredAssistantResponseValidator, StructuredAssistantResponseValidator>();
        services.AddSingleton<RuleBasedSafetyClassifier>();
        services.AddSingleton<AiSafetyClassifier>();
        services.AddSingleton<ISafetyClassifier, RoutingSafetyClassifier>();
        services.AddSingleton<IChatGuardrailEvaluator, ChatGuardrailEvaluator>();
        services.AddSingleton<RuleBasedIntentClassifier>();
        services.AddSingleton<AiIntentClassifier>();
        services.AddSingleton<IIntentClassifier, RoutingIntentClassifier>();
        services.AddSingleton<IAgentPlanner, AgentPlanner>();
        services.AddSingleton<IDocumentAgent, DocumentAgent>();
        services.AddSingleton<IEmailAgent, EmailAgent>();
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
        services.AddSingleton<IToolCallingService, SingleTurnToolCallingService>();

        return services;
    }

    // Skills are reusable AI capability modules that can be called by controllers, planners, or workflows.
    public static IServiceCollection AddSkills(this IServiceCollection services)
    {
        services.AddSingleton<ISummarySkill, SummarySkill>();
        services.AddSingleton<IRiskAnalysisSkill, RiskAnalysisSkill>();
        services.AddSingleton<IEmailDraftSkill, EmailDraftSkill>();
        services.AddSingleton<IClassificationSkill, ClassificationSkill>();
        services.AddSingleton<IResumeReviewSkill, ResumeReviewSkill>();

        return services;
    }

}
