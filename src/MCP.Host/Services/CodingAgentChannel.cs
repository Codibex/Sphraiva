using System.Threading.Channels;
using MCP.Host.Agents;

namespace MCP.Host.Services;

public class CodingAgentChannel(ILogger<CodingAgentChannel> logger) : ICodingAgentChannel
{
    private readonly Channel<CodingAgentImplementationTask> _channel = Channel.CreateUnbounded<CodingAgentImplementationTask>(new UnboundedChannelOptions()
    {
        SingleWriter = false,
        SingleReader = true
    });

    public void AddTask(CodingAgentImplementationTask task)
    {
        if (_channel.Writer.TryWrite(task))
        {
            logger.LogInformation("Task added");
        }
    }

    public IAsyncEnumerable<CodingAgentImplementationTask> ReadAllTasksAsync(CancellationToken cancellationToken)
        => _channel.Reader.ReadAllAsync(cancellationToken);
}