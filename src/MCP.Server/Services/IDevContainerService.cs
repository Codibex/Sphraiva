using MCP.Server.Common;

namespace MCP.Server.Services;

public interface IDevContainerService
{
    Task<OperationResult<string>> CreateDevContainerAsync(GitConfig gitConfig);
    Task<OperationResult> CleanupDevContainerAsync(string containerName);
}