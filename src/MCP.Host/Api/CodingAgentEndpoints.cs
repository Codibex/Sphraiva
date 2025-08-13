using MCP.Host.Agents;
using MCP.Host.Chat;
using MCP.Host.Contracts;
using MCP.Host.Services;

namespace MCP.Host.Api;

public static class CodingAgentEndpoints
{
    public static void MapCodingAgentEndpoints(this WebApplication app)
    {
        app.MapPost("/agent/workflow/approve",
            async (HeaderValueProvider headerValueProvider, ICodingAgentProcessStore store, CodingAgentImplementationApprovalRequest request) =>
            {
                var chatId = headerValueProvider.ChatId;
                if (store.TryGetProcess(chatId!.Value, out var process))
                {
                    await process.UserApprovedDocumentAsync(request.Approve);
                }
            }
        ).AddEndpointFilter<RequireChatIdEndpointFilter>();

        app.MapPost("/agent/workflow",
                async (
                    HeaderValueProvider headerValueProvider,
                    ICodingAgentProcessStore store,
                    CodingAgentImplementationRequest request,
                    ICodingAgentChannel channel) =>
                {
                    var codingAgentHubConnectionId = headerValueProvider.CodingAgentHubConnectionId;
                    var chatId = headerValueProvider.ChatId;

                    if (store.TryGetProcess(chatId!.Value, out var process))
                    {
                        await process.ContinueAsync(
                            new CodingAgentImplementationTask(chatId!.Value, codingAgentHubConnectionId!,
                                request.Requirement));
                    }
                    else
                    {
                        channel.AddTask(new CodingAgentImplementationTask(chatId!.Value, codingAgentHubConnectionId!,
                            request.Requirement));
                    }
                    
                    return Results.Ok("Starting Implementation");
                })
            .AddEndpointFilter<RequireChatIdEndpointFilter>()
            .AddEndpointFilter<RequireCodingAgentHubConnectionIdEndpointFilter>();
    }
}