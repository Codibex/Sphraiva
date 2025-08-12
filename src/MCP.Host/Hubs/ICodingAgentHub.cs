namespace MCP.Host.Hubs;

public interface ICodingAgentHub
{
    Task ReceiveMissingParametersAsync(ICollection<string> missingParameters);

    Task ReceiveUserReviewAsync(string document);
}