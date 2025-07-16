using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;

namespace MCP.Host.Services;

public class KernelProvider(Kernel kernel, IConfiguration configuration) : IKernelProvider
{
    public async Task<Kernel> GetAsync()
    {
        var mcpClient = await McpClientFactory.CreateAsync(new SseClientTransport(new SseClientTransportOptions()
        {
            Endpoint = new Uri(configuration["MCP_SERVER"]!),
            TransportMode = HttpTransportMode.AutoDetect
        }));

        var tools = await mcpClient.ListToolsAsync().ConfigureAwait(false);
        // var resources = await mcpClient.ListResourcesAsync().ConfigureAwait(false);
        // var templateResources = await mcpClient.ListResourceTemplatesAsync().ConfigureAwait(false);

        kernel.Plugins.AddFromFunctions("Sphraiva", tools.Select(t => t.AsKernelFunction()));
        return kernel;
    }
}
