using EnterpriseAiDocumentAssistant.Api.Options;
using EnterpriseAiDocumentAssistant.Api.Guardrails;
using EnterpriseAiDocumentAssistant.Api.PromptOrchestration;
using EnterpriseAiDocumentAssistant.Api.Services;
using EnterpriseAiDocumentAssistant.Api.StructuredOutput;
using EnterpriseAiDocumentAssistant.Api.ToolGateway;
using EnterpriseAiDocumentAssistant.Api.ToolGateway.Tools;
using EnterpriseAiDocumentAssistant.Api.Swagger;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FrontendOptions>(
    builder.Configuration.GetSection(FrontendOptions.SectionName));
builder.Services.Configure<AiGatewayOptions>(
    builder.Configuration.GetSection(AiGatewayOptions.SectionName));

builder.Services.AddSingleton<ISystemClock, SystemClock>();
builder.Services.AddSingleton<IApiStatusProvider, ApiStatusProvider>();
builder.Services.AddSingleton<IWorkspaceDataProvider, WorkspaceDataProvider>();
builder.Services.AddSingleton<IDocumentAssistantPromptOrchestrator, DocumentAssistantPromptOrchestrator>();
builder.Services.AddSingleton<IStructuredAssistantResponseValidator, StructuredAssistantResponseValidator>();
builder.Services.AddSingleton<IChatGuardrailEvaluator, ChatGuardrailEvaluator>();
builder.Services.AddSingleton<ITool, GetHealthStatusTool>();
builder.Services.AddSingleton<ITool, GetDocumentMetadataTool>();
builder.Services.AddSingleton<IToolRegistry, InMemoryToolRegistry>();
builder.Services.AddSingleton<IToolExecutor, ToolExecutor>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<ToolExecuteExampleOperationFilter>();
});

builder.Services.AddCors(options =>
{
    var frontendOptions = builder.Configuration
        .GetSection(FrontendOptions.SectionName)
        .Get<FrontendOptions>() ?? new FrontendOptions();

    options.AddPolicy(FrontendOptions.CorsPolicyName, policy =>
    {
        policy
            .WithOrigins(frontendOptions.AllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        var logger = context.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("GlobalExceptionHandler");

        if (exceptionFeature?.Error is not null)
        {
            logger.LogError(
                exceptionFeature.Error,
                "Unhandled exception while processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path);
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "UnexpectedError",
            Detail = "An unexpected error occurred."
        });
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(FrontendOptions.CorsPolicyName);
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
