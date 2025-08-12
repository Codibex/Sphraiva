namespace MCP.WebApp.Client.Components;

public class CodingAgentViewModel(Guid chatId, string displayName)
{
    public Guid ChatId { get; } = chatId;
    public string DisplayName { get; } = displayName;
    public IList<ChatMessageViewModel> Messages { get; set; } = [];
}
