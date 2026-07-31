using EnterpriseAiDocumentAssistant.Api.Security;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EnterpriseAiDocumentAssistant.Api.Swagger;

public sealed class CurrentUserHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Swagger exposes the local identity header so owner and reader behavior can be tested directly.
        operation.Parameters ??= [];
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = HttpHeaderCurrentUserAccessor.HeaderName,
            In = ParameterLocation.Header,
            Required = false,
            Description = "Local user identity. Defaults to 'local-user'.",
            Schema = new OpenApiSchema
            {
                Type = "string",
                Default = new Microsoft.OpenApi.Any.OpenApiString(HttpHeaderCurrentUserAccessor.DefaultUserId)
            }
        });
    }
}
