using ModelContextProtocol.Client;

namespace MCP.Host.Plugins;

public class McpPluginCacheBackgroundService(
    IMcpPluginCache pluginCache,
    IConfiguration configuration,
    ILogger<McpPluginCacheBackgroundService> logger)
    : BackgroundService
{
    private const int MAX_HEALTH_CHECK_ATTEMPTS = 10;
    private const int HEALTH_CHECK_DELAY_SECONDS = 2;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var endpoint = configuration["MCP_SERVER"];
        if (string.IsNullOrEmpty(endpoint))
        {
            logger.LogError("Configuration value for 'MCP_SERVER' is missing or empty.");
            return;
        }
        if (!await WaitForServiceAvailableAsync(endpoint, stoppingToken))
        {
            logger.LogError("MCP Server at {Endpoint} was not available after {Attempts} attempts.", endpoint, MAX_HEALTH_CHECK_ATTEMPTS);
            return;
        }

        var tools = await FetchToolsAsync(endpoint, stoppingToken);

        // var resources = await mcpClient.ListResourcesAsync().ConfigureAwait(false);
        // var templateResources = await mcpClient.ListResourceTemplatesAsync().ConfigureAwait(false);

        pluginCache.UpdateToolsForPlugin(PluginNames.Sphraiva, tools.AsReadOnly());
    }

    private static async Task<bool> WaitForServiceAvailableAsync(string endpoint, CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();
        var healthUrl = endpoint.TrimEnd('/') + "/health";

        for (var attempt = 1; attempt <= MAX_HEALTH_CHECK_ATTEMPTS && !cancellationToken.IsCancellationRequested; attempt++)
        {
            try
            {
                var response = await httpClient.GetAsync(healthUrl, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
            {
                // Ignore, Retry follows
            }

            await Task.Delay(TimeSpan.FromSeconds(HEALTH_CHECK_DELAY_SECONDS), cancellationToken);
        }

        return false;
    }

    private static async Task<IList<McpClientTool>> FetchToolsAsync(string endpoint, CancellationToken cancellationToken)
    {
        var mcpClient = await McpClientFactory.CreateAsync(new SseClientTransport(new SseClientTransportOptions
        {
            Endpoint = new Uri(endpoint),
            TransportMode = HttpTransportMode.AutoDetect
        }), cancellationToken: cancellationToken);

        return await mcpClient.ListToolsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}