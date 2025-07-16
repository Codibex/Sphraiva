using System.Collections.Concurrent;
using ModelContextProtocol.Client;

namespace MCP.Host.Plugins;

public class McpPluginCache : IMcpPluginCache
{
    private readonly ConcurrentDictionary<string, IReadOnlyList<McpClientTool>> _toolsByPlugin = new();

    public Task<IReadOnlyList<McpClientTool>> GetToolsForPluginAsync(string pluginName)
    {
        _toolsByPlugin.TryGetValue(pluginName, out var tools);
        return Task.FromResult(tools ?? new List<McpClientTool>());
    }

    public Task UpdateToolsForPluginAsync(string pluginName, IReadOnlyList<McpClientTool> tools)
    {
        _toolsByPlugin[pluginName] = tools;
        return Task.CompletedTask;
    }
}
