using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EnterpriseAiDocumentAssistant.Api.Swagger;

public sealed class ToolExecuteExampleOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!string.Equals(context.ApiDescription.RelativePath, "api/tools/execute", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!string.Equals(context.ApiDescription.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!operation.RequestBody.Content.TryGetValue("application/json", out var mediaType))
        {
            return;
        }

        mediaType.Example = new OpenApiObject
        {
            ["toolName"] = new OpenApiString("get_document_metadata"),
            ["arguments"] = new OpenApiObject
            {
                ["documentId"] = new OpenApiString("contract-review")
            }
        };
    }
}
