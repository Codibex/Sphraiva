using MCP.Host.Api;
using Microsoft.AspNetCore.SignalR;

namespace MCP.Host.Hubs;

public class CodeAgentHub : Hub<ICodeAgentHub>
{
    public async Task SendUserReviewAsync(string connectionId, string document)
    {
        var client = Clients.Client(connectionId);
        await client.ReceiveUserReviewAsync(document);
    }
}

public interface ICodeAgentHub
{
    Task ReceiveUserReviewAsync(string document);
}
