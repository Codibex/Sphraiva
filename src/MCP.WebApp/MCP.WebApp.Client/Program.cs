using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

await builder.Build().RunAsync();

public interface IMcpHttpClient
{
    Task<string> ChatAsync(string message);
}

public class McpHttpClient(HttpClient httpClient) : IMcpHttpClient
{
    public async Task<string> ChatAsync(string message)
    {
        var response = await httpClient.PostAsJsonAsync("chat", new
        {
            message = message
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}

public interface IMcpService
{
    Task<string> ChatAsync(string message);
}

public class McpService(IMcpHttpClient mcpHttpClient) : IMcpService
{
    public async Task<string> ChatAsync(string message)
    {
        return await mcpHttpClient.ChatAsync(message);
    }
}