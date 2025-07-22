using MCP.Host.Contracts;

namespace MCP.Host.Chat;

/// <summary>
/// Endpoint filter to require a valid CodingAgentHubConnectionId in HeaderValueProvider.
/// </summary>
public class RequireCodingAgentHubConnectionIdEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var headerValueProvider = context.Arguments.OfType<HeaderValueProvider>().FirstOrDefault();
        if (headerValueProvider?.CodingAgentHubConnectionId is null)
        {
            return Results.BadRequest($"Missing {HeaderNames.CODING_AGENT_HUB_CONNECTION_ID_HEADER_NAME} header");
        }
        return await next(context);
    }
}