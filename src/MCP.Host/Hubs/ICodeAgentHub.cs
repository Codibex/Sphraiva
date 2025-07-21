namespace MCP.Host.Hubs;

public interface ICodeAgentHub
{
    Task ReceiveUserReviewAsync(string document);
}