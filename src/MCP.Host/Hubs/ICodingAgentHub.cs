namespace MCP.Host.Hubs;

public interface ICodingAgentHub
{
    Task ReceiveMissingParametersAsync(ICollection<string> missingParameters);
    Task ReceiveImplementationUpdateAsync(string message);
    Task ReceiveUserReviewAsync(string document);
}