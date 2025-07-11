using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

await builder.Build().RunAsync();

public interface IMcpHttpClient
{
    Task<string> ChatAsync(string message, CancellationToken cancellationToken);
    Task<string> AgentAsync(string message, CancellationToken cancellationToken);
}

public class McpHttpClient(HttpClient httpClient) : IMcpHttpClient
{
    public async Task<string> ChatAsync(string message, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("chat", new
        {
            message = message
        }, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<string> AgentAsync(string message, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("agent", new
        {
            message = message
        }, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}

public interface IMcpService
{
    Task<string> ChatAsync(string message, CancellationToken cancellationToken);
    Task<string> AgentAsync(string message, CancellationToken cancellationToken);
}

public class McpService(IMcpHttpClient mcpHttpClient) : IMcpService
{
    public async Task<string> ChatAsync(string message, CancellationToken cancellationToken)
    {
        return await mcpHttpClient.ChatAsync(message, cancellationToken);
    }

    public async Task<string> AgentAsync(string message, CancellationToken cancellationToken)
    {
        return await mcpHttpClient.AgentAsync(message, cancellationToken);
    }
}