using MCP.Host.Agents;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace MCP.Host.Services;

public class CodingAgentProcessStore : ICodingAgentProcessStore
{
    private readonly ConcurrentDictionary<Guid, CodingAgentProcess> _processes = new();
    private readonly ConcurrentDictionary<Guid, CodingFlowProcess> _flows = new();

    public void AddProcess(Guid chatId, CodingAgentProcess process)
        => _processes[chatId] = process;

    public bool TryGetProcess(Guid chatId, [NotNullWhen(true)] out CodingAgentProcess? process)
        => _processes.TryGetValue(chatId, out process);

    public bool RemoveProcess(Guid chatId)
        => _processes.TryRemove(chatId, out _);

    public void AddFlow(Guid chatId, CodingFlowProcess process)
        => _flows[chatId] = process;

    public bool TryGetFlow(Guid chatId, [NotNullWhen(true)] out CodingFlowProcess? process)
        => _flows.TryGetValue(chatId, out process);

    public bool RemoveFlow(Guid chatId)
        => _flows.TryRemove(chatId, out _);
}