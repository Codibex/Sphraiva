using MCP.Host.Agents;

namespace MCP.Host.Services;

public interface ICodingAgentChannel
{
    void AddTask(CodingAgentImplementationTask task);
    IAsyncEnumerable<CodingAgentImplementationTask> ReadAllTasksAsync(CancellationToken cancellationToken);
}