using EnterpriseAiDocumentAssistant.Api.Guardrails;
using EnterpriseAiDocumentAssistant.Api.Harness;
using EnterpriseAiDocumentAssistant.Api.PromptOrchestration;
using EnterpriseAiDocumentAssistant.Api.Services;
using EnterpriseAiDocumentAssistant.Api.Skills;
using EnterpriseAiDocumentAssistant.Api.StructuredOutput;
using EnterpriseAiDocumentAssistant.Api.ToolGateway;
using EnterpriseAiDocumentAssistant.Api.ToolGateway.Tools;

namespace EnterpriseAiDocumentAssistant.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<ISystemClock, SystemClock>();
        services.AddSingleton<IApiStatusProvider, ApiStatusProvider>();
        services.AddSingleton<IWorkspaceDataProvider, WorkspaceDataProvider>();
        services.AddSingleton<IDocumentAssistantPromptOrchestrator, DocumentAssistantPromptOrchestrator>();
        services.AddSingleton<IStructuredAssistantResponseValidator, StructuredAssistantResponseValidator>();
        services.AddSingleton<IChatGuardrailEvaluator, ChatGuardrailEvaluator>();
        services.AddSingleton<IHarnessRunner, HarnessRunner>();

        return services;
    }

    public static IServiceCollection AddToolGateway(this IServiceCollection services)
    {
        services.AddSingleton<ITool, GetHealthStatusTool>();
        services.AddSingleton<ITool, GetDocumentMetadataTool>();
        services.AddSingleton<IToolRegistry, InMemoryToolRegistry>();
        services.AddSingleton<IToolExecutor, ToolExecutor>();

        return services;
    }

    public static IServiceCollection AddSkills(this IServiceCollection services)
    {
        services.AddSingleton<ISummarySkill, SummarySkill>();
        services.AddSingleton<IRiskAnalysisSkill, RiskAnalysisSkill>();
        services.AddSingleton<IEmailDraftSkill, EmailDraftSkill>();

        return services;
    }
}
