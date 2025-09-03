using System.Diagnostics.CodeAnalysis;
using MCP.Host.Agents.CodingAgent;

namespace MCP.Host.Services;

public interface ICodingAgentWorkflowStore
{
    void AddFlow(Guid chatId, CodingAgentWorkflow process);
    public bool TryGetFlow(Guid chatId, [NotNullWhen(true)] out CodingAgentWorkflow? process);
    public bool RemoveFlow(Guid chatId);
}