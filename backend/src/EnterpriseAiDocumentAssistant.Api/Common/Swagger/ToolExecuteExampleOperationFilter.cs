using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EnterpriseAiDocumentAssistant.Api.Swagger;

public sealed class ToolExecuteExampleOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Swagger examples keep manual API testing fast while the backend still exposes normal JSON contracts.
        if (!string.Equals(context.ApiDescription.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!operation.RequestBody.Content.TryGetValue("application/json", out var mediaType))
        {
            return;
        }

        if (string.Equals(context.ApiDescription.RelativePath, "api/tools/execute", StringComparison.OrdinalIgnoreCase))
        {
            mediaType.Example = new OpenApiObject
            {
                ["toolName"] = new OpenApiString("get_health_status"),
                ["arguments"] = new OpenApiObject()
            };

            return;
        }

        if (string.Equals(context.ApiDescription.RelativePath, "api/mcp/tools/call", StringComparison.OrdinalIgnoreCase))
        {
            mediaType.Example = new OpenApiObject
            {
                ["name"] = new OpenApiString("get_health_status"),
                ["arguments"] = new OpenApiObject()
            };

            return;
        }

        if (string.Equals(context.ApiDescription.RelativePath, "api/skills/summary", StringComparison.OrdinalIgnoreCase))
        {
            mediaType.Example = new OpenApiObject
            {
                ["documentId"] = new OpenApiString("upload-document-id"),
                ["aiProvider"] = new OpenApiString("OpenAI")
            };

            return;
        }

        if (string.Equals(context.ApiDescription.RelativePath, "api/skills/risk-analysis", StringComparison.OrdinalIgnoreCase))
        {
            mediaType.Example = new OpenApiObject
            {
                ["documentId"] = new OpenApiString("upload-document-id"),
                ["aiProvider"] = new OpenApiString("OpenAI")
            };

            return;
        }

        if (string.Equals(context.ApiDescription.RelativePath, "api/skills/email-draft", StringComparison.OrdinalIgnoreCase))
        {
            mediaType.Example = new OpenApiObject
            {
                ["documentId"] = new OpenApiString("upload-document-id"),
                ["purpose"] = new OpenApiString("Ask the vendor to clarify renewal, liability, and service credit terms."),
                ["aiProvider"] = new OpenApiString("OpenAI")
            };

            return;
        }

        if (string.Equals(context.ApiDescription.RelativePath, "api/skills/resume-review", StringComparison.OrdinalIgnoreCase))
        {
            mediaType.Example = new OpenApiObject
            {
                ["documentId"] = new OpenApiString("upload-document-id"),
                ["instruction"] = new OpenApiString("Create a practical resume review brief that I can use with the original resume in ChatGPT."),
                ["aiProvider"] = new OpenApiString("OpenAI")
            };

            return;
        }

        if (string.Equals(context.ApiDescription.RelativePath, "api/skills/classification", StringComparison.OrdinalIgnoreCase))
        {
            mediaType.Example = new OpenApiObject
            {
                ["documentId"] = new OpenApiString("upload-document-id"),
                ["aiProvider"] = new OpenApiString("OpenAI")
            };

            return;
        }

        if (string.Equals(context.ApiDescription.RelativePath, "api/planner/plan", StringComparison.OrdinalIgnoreCase))
        {
            mediaType.Example = new OpenApiObject
            {
                ["message"] = new OpenApiString("Analyze liability risk in this document."),
                ["documentId"] = new OpenApiString("upload-document-id")
            };

            return;
        }

        if (string.Equals(context.ApiDescription.RelativePath, "api/chat", StringComparison.OrdinalIgnoreCase)
            || string.Equals(context.ApiDescription.RelativePath, "api/chat/structured", StringComparison.OrdinalIgnoreCase)
            || string.Equals(context.ApiDescription.RelativePath, "api/chat/stream", StringComparison.OrdinalIgnoreCase))
        {
            mediaType.Example = new OpenApiObject
            {
                ["message"] = new OpenApiString("What should I review first in this agreement?"),
                ["documentId"] = new OpenApiString("upload-document-id"),
                ["history"] = new OpenApiArray(),
                ["aiProvider"] = new OpenApiString("OpenAI")
            };

            return;
        }

        if (string.Equals(context.ApiDescription.RelativePath, "api/workflows/document-review", StringComparison.OrdinalIgnoreCase))
        {
            mediaType.Example = new OpenApiObject
            {
                ["documentId"] = new OpenApiString("upload-document-id"),
                ["emailPurpose"] = new OpenApiString("Ask the vendor to clarify renewal, liability, and service credit terms."),
                ["aiProvider"] = new OpenApiString("OpenAI")
            };

            return;
        }

        if (string.Equals(context.ApiDescription.RelativePath, "api/integrations/graph/email-draft", StringComparison.OrdinalIgnoreCase))
        {
            mediaType.Example = new OpenApiObject
            {
                ["documentId"] = new OpenApiString("upload-document-id"),
                ["to"] = new OpenApiString("vendor@example.com"),
                ["subject"] = new OpenApiString("Questions about Vendor Service Agreement"),
                ["body"] = new OpenApiString("Please clarify renewal, liability, and service credit terms before approval.")
            };
        }
    }
}
