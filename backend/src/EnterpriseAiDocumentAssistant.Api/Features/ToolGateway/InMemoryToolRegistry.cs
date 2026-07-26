namespace EnterpriseAiDocumentAssistant.Api.ToolGateway;

public sealed class InMemoryToolRegistry : IToolRegistry
{
    private readonly IReadOnlyDictionary<string, ITool> toolsByName;

    public InMemoryToolRegistry(IEnumerable<ITool> tools)
    {
        // DI supplies every registered ITool; the registry turns that set into a lookup by tool name.
        toolsByName = tools.ToDictionary(
            tool => tool.Definition.Name,
            StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<ToolDefinition> ListDefinitions()
    {
        // List is the discoverability side of Tool Gateway and MCP.
        return toolsByName.Values
            .Select(tool => tool.Definition)
            .OrderBy(definition => definition.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public bool TryGetTool(string toolName, out ITool? tool)
    {
        // Execute uses the same name returned by ListDefinitions to resolve the concrete tool instance.
        return toolsByName.TryGetValue(toolName, out tool);
    }
}
