using Microsoft.SemanticKernel.ChatCompletion;

namespace MCP.Host.Services;

public class ChatHistoryProvider : IChatHistoryProvider
{
    private readonly ChatHistory _history = [];

    public ChatHistory Get() => _history;
}