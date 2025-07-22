using MCP.Host.Agents;

namespace MCP.Host.Services;

public class CodingAgentBackgroundService(ICodingAgentChannel channel, IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var implementationTask in channel.ReadAllTasksAsync(stoppingToken))
        {
            _ = Task.Run(async () =>
            {
                await using var scope = serviceProvider.CreateAsyncScope();
                var process = scope.ServiceProvider.GetRequiredService<CodingAgentProcess>();
                var processStore = scope.ServiceProvider.GetRequiredService<ICodingAgentProcessStore>();
                processStore.AddProcess(implementationTask.ChatId, process);
                await process.RunAsync(implementationTask, stoppingToken);
            }, stoppingToken);
        }
    }
}