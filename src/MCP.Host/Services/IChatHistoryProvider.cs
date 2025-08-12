using Microsoft.SemanticKernel.ChatCompletion;

namespace MCP.Host.Services;

public interface IChatHistoryProvider
{
    ChatHistory Get();
}