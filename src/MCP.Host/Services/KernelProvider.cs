using Microsoft.SemanticKernel;
using MCP.Host.Plugins;

namespace MCP.Host.Services;

public class KernelProvider(Kernel kernel, IMcpPluginCache pluginCache) : IKernelProvider
{
    public Kernel Get()
    {
        var tools = pluginCache.GetToolsForPlugin(PluginDescriptions.SphraivaPlugin.NAME);
        kernel.Plugins.AddFromFunctions(PluginDescriptions.SphraivaPlugin.NAME, tools.Select(t => t.AsKernelFunction()));
        return kernel;
    }
}
