namespace MCP.WebApp.Client.Services;

public class McpService(IMcpHttpClient mcpHttpClient) : IMcpService
{
    public async Task<string> ChatAsync(string message, CancellationToken cancellationToken)
    {
        return await mcpHttpClient.ChatAsync(message, cancellationToken);
    }

    public async Task AgentStreamAsync(string message, Action<string> onChunk, CancellationToken cancellationToken)
    {
        await mcpHttpClient.AgentStreamAsync(message, onChunk, cancellationToken);
    }
}