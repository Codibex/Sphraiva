namespace MCP.WebApp.Client.Components;

public class ChatMessageViewModel(string user, string text, DateTime timestamp, bool isUser)
{
    public string User { get; } = user;
    public string Text { get; set; } = text;
    public DateTime Timestamp { get; } = timestamp;
    public bool IsUser { get; } = isUser;

    public static ChatMessageViewModel CreateUserMessage(string text) => new ChatMessageViewModel("User", text, DateTime.Now, true);
    public static ChatMessageViewModel CreateAgentMessage(string text) => new ChatMessageViewModel("Agent", text, DateTime.Now, false);

    public string GetTimeString() => Timestamp.ToShortTimeString();
}