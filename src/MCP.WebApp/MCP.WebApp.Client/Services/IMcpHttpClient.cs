namespace MCP.WebApp.Client.Services;

public interface IMcpHttpClient
{
    Task<string> ChatAsync(string message, CancellationToken cancellationToken);
    Task AgentStreamAsync(string message, Action<string> onChunk, CancellationToken cancellationToken);
}