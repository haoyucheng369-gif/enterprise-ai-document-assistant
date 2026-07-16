namespace EnterpriseAiDocumentAssistant.Api.ToolGateway;

public sealed class InMemoryToolRegistry : IToolRegistry
{
    private readonly IReadOnlyDictionary<string, ITool> toolsByName;

    public InMemoryToolRegistry(IEnumerable<ITool> tools)
    {
        toolsByName = tools.ToDictionary(
            tool => tool.Definition.Name,
            StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<ToolDefinition> ListDefinitions()
    {
        return toolsByName.Values
            .Select(tool => tool.Definition)
            .OrderBy(definition => definition.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public bool TryGetTool(string toolName, out ITool? tool)
    {
        return toolsByName.TryGetValue(toolName, out tool);
    }
}
