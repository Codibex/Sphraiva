namespace MCP.WebApp.Client.Services;

public interface IMcpService
{
    Task<string> ChatAsync(string message, CancellationToken cancellationToken);
    Task AgentStreamAsync(Guid chatId, string message, Action<string> onChunk, CancellationToken cancellationToken);
    Task RemoveChatAsync(Guid chatId, CancellationToken cancellationToken);
}