using ModelContextProtocol.Client;

namespace MCP.Host.Plugins;

public class McpPluginCacheBackgroundService(IMcpPluginCache pluginCache, IConfiguration configuration)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var mcpClient = await McpClientFactory.CreateAsync(new SseClientTransport(new SseClientTransportOptions
        {
            Endpoint = new Uri(configuration["MCP_SERVER"]!),
            TransportMode = HttpTransportMode.AutoDetect
        }), cancellationToken: stoppingToken);

        var tools = await mcpClient.ListToolsAsync(cancellationToken: stoppingToken).ConfigureAwait(false);
        await pluginCache.UpdateToolsForPluginAsync(PluginNames.Sphraiva, tools.AsReadOnly());

        // var resources = await mcpClient.ListResourcesAsync().ConfigureAwait(false);
        // // var templateResources = await mcpClient.ListResourceTemplatesAsync().ConfigureAwait(false);
    }
}
