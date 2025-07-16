namespace MCP.Host.Chat;

/// <summary>
/// Endpoint filter to require a valid ChatId in HeaderValueProvider.
/// </summary>
public class RequireChatIdEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var headerValueProvider = context.Arguments.OfType<HeaderValueProvider>().FirstOrDefault();
        if (headerValueProvider?.ChatId is null)
        {
            return Results.BadRequest($"Missing {HeaderValueProvider.ChatIdHeaderName} header");
        }
        return await next(context);
    }
}
