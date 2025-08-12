using MCP.Host.Agents;

namespace MCP.Host.Services;

public class CodingAgentBackgroundService(ICodingAgentChannel channel, IServiceProvider serviceProvider, ILogger<CodingAgentBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var implementationTask in channel.ReadAllTasksAsync(stoppingToken))
        {
            //_ = Task.Run(async () =>
            //{
            //    await using var scope = serviceProvider.CreateAsyncScope();
            //    var process = scope.ServiceProvider.GetRequiredService<CodingAgentProcess>();
            //    var processStore = scope.ServiceProvider.GetRequiredService<ICodingAgentProcessStore>();
            //    processStore.AddProcess(implementationTask.ChatId, process);
            //    try
            //    {
            //        await process.RunAsync(implementationTask, stoppingToken);
            //    }
            //    catch (Exception e)
            //    {
            //        logger.LogError(e, "Unexpected agent process error.");
            //    }
            //}, stoppingToken);

            _ = Task.Run(async () =>
            {
                await using var scope = serviceProvider.CreateAsyncScope();
                var process = scope.ServiceProvider.GetRequiredService<CodingFlowProcess>();
                var processStore = scope.ServiceProvider.GetRequiredService<ICodingAgentProcessStore>();
                processStore.AddFlow(implementationTask.ChatId, process);
                try
                {
                    await process.RunAsync(new FlowParameter(implementationTask.ChatId, implementationTask.ConnectionId, implementationTask.Requirement));
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Unexpected agent process error.");
                }
            }, stoppingToken);
        }
    }
}