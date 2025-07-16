namespace MCP.Host.Chat;

/// <summary>
/// Background service to clean up inactive chat threads from the ChatCache.
/// </summary>
public class ChatCacheCleanupService(ChatCache chatCache, ILogger<ChatCacheCleanupService> logger)
    : BackgroundService
{
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(10);
    private readonly TimeSpan _expiration = TimeSpan.FromMinutes(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                chatCache.Cleanup(DateTime.UtcNow - _expiration);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during chat cache cleanup");
            }
            await Task.Delay(_cleanupInterval, stoppingToken);
        }
    }
}
