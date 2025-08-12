using MCP.Host.Contracts;

namespace MCP.Host.Chat;

/// <summary>
/// Middleware to extract header values (e.g. sphraiva-chat-id) and populate the HeaderValueProvider.
/// </summary>
public class HeaderValueProviderMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, HeaderValueProvider headerValueProvider)
    {
        SetChatId(headerValueProvider, context);
        SetCodingAgentHubConnectionId(headerValueProvider, context);
        await next(context);
    }

    private static void SetChatId(HeaderValueProvider headerValueProvider, HttpContext context)
    {
        var chatId = context.Request.Headers[HeaderNames.CHAT_ID_HEADER_NAME].FirstOrDefault();
        headerValueProvider.ChatId = chatId is null ? null : Guid.Parse(chatId);
    }

    private static void SetCodingAgentHubConnectionId(HeaderValueProvider headerValueProvider, HttpContext context)
    {
        var codingAgentHubConnectionId = context.Request.Headers[HeaderNames.CODING_AGENT_HUB_CONNECTION_ID_HEADER_NAME].FirstOrDefault();
        headerValueProvider.CodingAgentHubConnectionId = codingAgentHubConnectionId;
    }
}
