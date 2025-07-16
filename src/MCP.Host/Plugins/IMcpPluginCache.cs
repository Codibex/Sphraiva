using ModelContextProtocol.Client;

namespace MCP.Host.Plugins;

public interface IMcpPluginCache
{
    IReadOnlyList<McpClientTool> GetToolsForPlugin(string pluginName);
    void UpdateToolsForPlugin(string pluginName, IReadOnlyList<McpClientTool> tools);
}