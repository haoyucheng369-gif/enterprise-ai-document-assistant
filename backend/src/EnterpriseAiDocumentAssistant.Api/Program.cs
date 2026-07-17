using EnterpriseAiDocumentAssistant.Api.Options;
using EnterpriseAiDocumentAssistant.Api.Extensions;
using EnterpriseAiDocumentAssistant.Api.Swagger;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Bind runtime options early so infrastructure and CORS read from one configuration source.
builder.Services.Configure<FrontendOptions>(
    builder.Configuration.GetSection(FrontendOptions.SectionName));
builder.Services.Configure<AiGatewayOptions>(
    builder.Configuration.GetSection(AiGatewayOptions.SectionName));

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
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<ToolExecuteExampleOperationFilter>();
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

// Swagger remains development-only while the API contracts stay available through controllers.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(FrontendOptions.CorsPolicyName);
app.UseAuthorization();

// Health checks and controller routes are the public HTTP surface for this API.
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
