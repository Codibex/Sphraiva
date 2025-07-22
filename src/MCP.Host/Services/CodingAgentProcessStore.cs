using MCP.Host.Agents;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace MCP.Host.Services;

public class CodingAgentProcessStore : ICodingAgentProcessStore
{
    private readonly ConcurrentDictionary<Guid, CodingAgentProcess> _processes = new();

    public void AddProcess(Guid chatId, CodingAgentProcess process)
        => _processes[chatId] = process;

    public bool TryGetProcess(Guid chatId, [NotNullWhen(true)] out CodingAgentProcess? process)
        => _processes.TryGetValue(chatId, out process);

    public bool RemoveProcess(Guid chatId)
        => _processes.TryRemove(chatId, out _);
}