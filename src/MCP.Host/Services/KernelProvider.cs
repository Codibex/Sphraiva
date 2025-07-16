using Microsoft.SemanticKernel;
using MCP.Host.Plugins;

namespace MCP.Host.Services;

public class KernelProvider(Kernel kernel, IMcpPluginCache pluginCache) : IKernelProvider
{
    public async Task<Kernel> GetAsync()
    {
        var tools = await pluginCache.GetToolsForPluginAsync(PluginNames.Sphraiva);
        kernel.Plugins.AddFromFunctions(PluginNames.Sphraiva, tools.Select(t => t.AsKernelFunction()));
        return kernel;
    }
}
