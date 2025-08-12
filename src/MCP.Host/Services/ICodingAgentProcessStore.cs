using System.Diagnostics.CodeAnalysis;
using MCP.Host.Agents;

namespace MCP.Host.Services;

public interface ICodingAgentProcessStore
{
    void AddProcess(Guid chatId, CodingAgentProcess process);
    bool TryGetProcess(Guid chatId, [NotNullWhen(true)] out CodingAgentProcess? process);
    bool RemoveProcess(Guid chatId);

    void AddFlow(Guid chatId, CodingFlowProcess process);
    public bool TryGetFlow(Guid chatId, [NotNullWhen(true)] out CodingFlowProcess? process);
    public bool RemoveFlow(Guid chatId);
}