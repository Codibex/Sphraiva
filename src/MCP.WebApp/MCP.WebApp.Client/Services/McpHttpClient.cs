using MCP.Host.Contracts;
using System.Net.Http.Json;

namespace MCP.WebApp.Client.Services;

public class McpHttpClient(HttpClient httpClient) : IMcpHttpClient
{
    public async Task<string> ChatAsync(string message, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "chat")
        {
            Content = JsonContent.Create(new ChatRequest(message))
        };
        var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task AgentStreamAsync(Guid chatId, string message, Action<string> onChunk,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "agent/chat");
        request.Headers.Add(HeaderNames.CHAT_ID_HEADER_NAME, chatId.ToString());
        request.Content = JsonContent.Create(new ChatRequest( message));

        var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        var buffer = new char[8096];
        int read;
        while ((read = await reader.ReadAsync(buffer, cancellationToken)) > 0)
        {
            var content = new ReadOnlySpan<char>(buffer, 0, read);
            onChunk(content.ToString());
        }
    }

    public async Task CodeAgentStreamAsync(Guid chatId, string codingAgentHubConnectionId, string message, Action<string> onChunk, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "agent/workflow");
        request.Headers.Add(HeaderNames.CHAT_ID_HEADER_NAME, chatId.ToString());
        request.Headers.Add(HeaderNames.CODING_AGENT_HUB_CONNECTION_ID_HEADER_NAME, codingAgentHubConnectionId);
        request.Content = JsonContent.Create(new CodingAgentImplementationRequest(message));
        var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync(cancellationToken);
        onChunk(result);
    }

    public async Task CodeAgentApproveStreamAsync(Guid chatId, bool approve, Action<string> onChunk, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "agent/workflow/approve");
        request.Headers.Add(HeaderNames.CHAT_ID_HEADER_NAME, chatId.ToString());

        request.Content = JsonContent.Create(new CodingAgentImplementationApprovalRequest(approve));
        var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync(cancellationToken);
        onChunk(result);
    }

    public async Task RemoveChatAsync(Guid chatId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, "agent/chat");
        request.Headers.Add(HeaderNames.CHAT_ID_HEADER_NAME, chatId.ToString());

        var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}