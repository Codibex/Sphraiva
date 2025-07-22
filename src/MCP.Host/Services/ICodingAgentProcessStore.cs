using System.Diagnostics.CodeAnalysis;
using MCP.Host.Agents;

namespace MCP.Host.Services;

public interface ICodingAgentProcessStore
{
    void AddProcess(Guid chatId, CodingAgentProcess process);
    bool TryGetProcess(Guid chatId, [NotNullWhen(true)] out CodingAgentProcess? process);
    bool RemoveProcess(Guid chatId);
}