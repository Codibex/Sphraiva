namespace MCP.WebApp.Client.Services;

public interface IMcpService
{
    Task<string> ChatAsync(string message, CancellationToken cancellationToken);
    Task AgentStreamAsync(string message, Action<string> onChunk, CancellationToken cancellationToken);
}