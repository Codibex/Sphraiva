using Microsoft.SemanticKernel;
using MCP.Host.Plugins;

namespace MCP.Host.Services;

public class KernelProvider(Kernel kernel, IMcpPluginCache pluginCache) : IKernelProvider
{
    public Kernel Get()
    {
        var tools = pluginCache.GetToolsForPlugin(PluginNames.Sphraiva);
        kernel.Plugins.AddFromFunctions(PluginNames.Sphraiva, tools.Select(t => t.AsKernelFunction()));
        return kernel;
    }
}
