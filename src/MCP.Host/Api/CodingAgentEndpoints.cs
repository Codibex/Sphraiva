using MCP.Host.Agents;
using MCP.Host.Chat;
using MCP.Host.Contracts;
using MCP.Host.Services;

namespace MCP.Host.Api;

public static class CodingAgentEndpoints
{
    public static void MapCodingAgentEndpoints(this WebApplication app)
    {
        app.MapPost("/coding-agent/workflows",
                (
                    HeaderValueProvider headerValueProvider,
                    ICodingAgentWorkflowStore store,
                    CodingAgentImplementationRequest request,
                    ICodingAgentChannel channel) =>
                {
                    var codingAgentHubConnectionId = headerValueProvider.CodingAgentHubConnectionId;
                    var chatId = headerValueProvider.ChatId;

                    channel.AddTask(new CodingAgentImplementationTask(
                        chatId!.Value, 
                        codingAgentHubConnectionId!,
                        request.Requirement));

                    return Results.Ok("Starting Implementation");
                })
            .AddEndpointFilter<RequireChatIdEndpointFilter>()
            .AddEndpointFilter<RequireCodingAgentHubConnectionIdEndpointFilter>();
    }
}