using MCP.Host.Contracts;
using System.Net.Http.Json;

namespace MCP.WebApp.Client.Services;

public class McpHttpClient(HttpClient httpClient) : IMcpHttpClient
{
    public async Task<string> ChatAsync(string message, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("chat", new
        {
            message
        }, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task AgentStreamAsync(string message, Action<string> onChunk, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("agent", new ChatRequest(message), cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                onChunk(line);
            }
        }
    }
}