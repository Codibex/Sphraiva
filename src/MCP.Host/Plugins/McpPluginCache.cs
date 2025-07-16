using System.Collections.Concurrent;
using ModelContextProtocol.Client;

namespace MCP.Host.Plugins;

public class McpPluginCache : IMcpPluginCache
{
    private readonly ConcurrentDictionary<string, IReadOnlyList<McpClientTool>> _toolsByPlugin = new();

    public IReadOnlyList<McpClientTool> GetToolsForPlugin(string pluginName)
    {
        _toolsByPlugin.TryGetValue(pluginName, out var tools);
        return tools ?? new List<McpClientTool>();
    }

    public void UpdateToolsForPlugin(string pluginName, IReadOnlyList<McpClientTool> tools)
    {
        _toolsByPlugin[pluginName] = tools;
    }
}
