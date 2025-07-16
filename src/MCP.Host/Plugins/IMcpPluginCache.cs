using ModelContextProtocol.Client;

namespace MCP.Host.Plugins;

public interface IMcpPluginCache
{
    Task<IReadOnlyList<McpClientTool>> GetToolsForPluginAsync(string pluginName);
    Task UpdateToolsForPluginAsync(string pluginName, IReadOnlyList<McpClientTool> tools);
}