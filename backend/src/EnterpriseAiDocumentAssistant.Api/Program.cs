using EnterpriseAiDocumentAssistant.Api.Options;
using EnterpriseAiDocumentAssistant.Api.Extensions;
using EnterpriseAiDocumentAssistant.Api.RateLimiting;
using EnterpriseAiDocumentAssistant.Api.Security;
using EnterpriseAiDocumentAssistant.Api.Swagger;
using Grpc.Core;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Local settings are optional and ignored by Git, so API keys stay on the developer machine.
builder.Configuration.AddJsonFile(
    "appsettings.Local.json",
    optional: true,
    reloadOnChange: true);

// Bind runtime options early so infrastructure and CORS read from one configuration source.
builder.Services.Configure<FrontendOptions>(
    builder.Configuration.GetSection(FrontendOptions.SectionName));
builder.Services.Configure<AiGatewayOptions>(
    builder.Configuration.GetSection(AiGatewayOptions.SectionName));
builder.Services.Configure<RagOptions>(
    builder.Configuration.GetSection(RagOptions.SectionName));
builder.Services.Configure<QdrantOptions>(
    builder.Configuration.GetSection(QdrantOptions.SectionName));
builder.Services.Configure<MongoDbOptions>(
    builder.Configuration.GetSection(MongoDbOptions.SectionName));

// Keep feature registrations grouped by application boundary instead of growing Program.cs.
builder.Services
    .AddApplicationServices()
    .AddToolGateway()
    .AddSkills();

// Register ASP.NET Core platform services used by controllers, health checks, errors, and Swagger.
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Partition by the current local identity so one user cannot consume every user's allowance.
    options.AddPolicy(AiRateLimitPolicy.Name, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.RequestServices.GetRequiredService<ICurrentUserAccessor>().UserId,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = AiRateLimitPolicy.PermitLimit,
                Window = AiRateLimitPolicy.Window,
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "RateLimitExceeded",
            Detail = "Too many AI requests. Try again after the current rate-limit window."
        }, cancellationToken);
    };
});
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<ToolExecuteExampleOperationFilter>();
    options.OperationFilter<CurrentUserHeaderOperationFilter>();
});

// Frontend origins stay configurable so local and deployed clients can use the same API setup.
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

// Centralize unexpected API failures as ProblemDetails without leaking exception internals.
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();

        // Map known infrastructure failures while keeping unexpected implementation details private.
        var (status, title, detail) = exceptionFeature?.Error switch
        {
            RpcException { StatusCode: Grpc.Core.StatusCode.Unavailable } =>
                (StatusCodes.Status503ServiceUnavailable, "DependencyUnavailable", "A required service is unavailable."),
            RpcException { StatusCode: Grpc.Core.StatusCode.DeadlineExceeded } =>
                (StatusCodes.Status504GatewayTimeout, "DependencyTimeout", "A required service timed out."),
            _ =>
                (StatusCodes.Status500InternalServerError, "UnexpectedError", "An unexpected error occurred.")
        };

        context.Response.StatusCode = status;

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail
        });
    });
});

// Swagger remains development-only while the API contracts stay available through controllers.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(FrontendOptions.CorsPolicyName);
app.UseRateLimiter();
app.UseAuthorization();

// Health checks and controller routes are the public HTTP surface for this API.
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
