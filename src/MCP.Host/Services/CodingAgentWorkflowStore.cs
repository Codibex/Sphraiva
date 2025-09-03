using MCP.Host.Agents.CodingAgent;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace MCP.Host.Services;

public class CodingAgentWorkflowStore : ICodingAgentWorkflowStore
{
    private readonly ConcurrentDictionary<Guid, CodingAgentWorkflow> _flows = new();

    public void AddFlow(Guid chatId, CodingAgentWorkflow process)
        => _flows[chatId] = process;

    public bool TryGetFlow(Guid chatId, [NotNullWhen(true)] out CodingAgentWorkflow? process)
        => _flows.TryGetValue(chatId, out process);

    public bool RemoveFlow(Guid chatId)
        => _flows.TryRemove(chatId, out _);
}