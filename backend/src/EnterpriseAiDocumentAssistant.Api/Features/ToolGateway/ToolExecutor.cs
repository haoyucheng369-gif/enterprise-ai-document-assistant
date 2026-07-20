using System.Diagnostics;
using EnterpriseAiDocumentAssistant.Api.Audit;

namespace EnterpriseAiDocumentAssistant.Api.ToolGateway;

public sealed class ToolExecutor : IToolExecutor
{
    private readonly IAuditLogger auditLogger;
    private readonly IToolRegistry toolRegistry;

    public ToolExecutor(
        IAuditLogger auditLogger,
        IToolRegistry toolRegistry)
    {
        this.auditLogger = auditLogger;
        this.toolRegistry = toolRegistry;
    }

    public Task<ToolExecutionResult> ExecuteAsync(
        ToolExecutionRequest request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        if (string.IsNullOrWhiteSpace(request.ToolName))
        {
            var result = new ToolExecutionResult(
                request.ToolName,
                false,
                "ToolName is required.",
                new Dictionary<string, object?>());

            RecordAudit(request.ToolName, result, stopwatch.ElapsedMilliseconds);
            return Task.FromResult(result);
        }

        if (!toolRegistry.TryGetTool(request.ToolName, out var tool) || tool is null)
        {
            var result = new ToolExecutionResult(
                request.ToolName,
                false,
                $"Tool '{request.ToolName}' is not registered.",
                new Dictionary<string, object?>());

            RecordAudit(request.ToolName, result, stopwatch.ElapsedMilliseconds);
            return Task.FromResult(result);
        }

        return ExecuteAndAuditAsync(tool, request, stopwatch, cancellationToken);
    }

    private async Task<ToolExecutionResult> ExecuteAndAuditAsync(
        ITool tool,
        ToolExecutionRequest request,
        Stopwatch stopwatch,
        CancellationToken cancellationToken)
    {
        var result = await tool.ExecuteAsync(request, cancellationToken);
        RecordAudit(request.ToolName, result, stopwatch.ElapsedMilliseconds);

        return result;
    }

    private void RecordAudit(string toolName, ToolExecutionResult result, long durationMs)
    {
        auditLogger.Record(new AuditEventRequest(
            "tool",
            "tool_executed",
            "tools.execute",
            result.Succeeded,
            durationMs,
            new Dictionary<string, string>
            {
                ["toolName"] = toolName,
                ["error"] = result.Error ?? string.Empty
            }));
    }
}
