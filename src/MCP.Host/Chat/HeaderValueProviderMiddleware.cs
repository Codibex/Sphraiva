using MCP.Host.Contracts;

namespace MCP.Host.Chat;

/// <summary>
/// Middleware to extract header values (e.g. sphraiva-chat-id) and populate the HeaderValueProvider.
/// </summary>
public class HeaderValueProviderMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, HeaderValueProvider headerValueProvider)
    {
        var chatId = context.Request.Headers[HeaderNames.ChatIdHeaderName].FirstOrDefault();
        headerValueProvider.ChatId = chatId is null ? null : Guid.Parse(chatId);
        await next(context);
    }
}
