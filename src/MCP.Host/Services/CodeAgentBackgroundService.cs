using System.Threading.Channels;

namespace MCP.Host.Services;

public class CodeAgentBackgroundService(Channel<CodeAgentImplementationTask> channel, IServiceProvider serviceProvider) : BackgroundService
{
    // TODO: Use own service which creates the channel and provides Add/get methods
    // Idea from https://jonathancrozier.com/blog/communicating-with-asp-dot-net-core-hosted-services-from-api-endpoints-using-channels
    //private readonly Channel<CodeAgentImplementationTask> _channel = Channel.CreateUnbounded<CodeAgentImplementationTask>(new UnboundedChannelOptions()
    //{
    //    SingleWriter = false,
    //    SingleReader = true
    //});

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var implementationTask in channel.Reader.ReadAllAsync(stoppingToken))
        {
            _ = Task.Run(async () =>
            {
                await using var scope = serviceProvider.CreateAsyncScope();
                var process = scope.ServiceProvider.GetRequiredService<CodeAgentProcess>();
                await process.RunAsync(implementationTask, stoppingToken);
            }, stoppingToken);
        }
    }
}