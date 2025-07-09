using MCP.Server.Common;

namespace MCP.Server.Services;

public interface IDevContainerService
{
    Task<OperationResult<string>> CreateDevContainerAsync();
    Task<OperationResult> CleanupDevContainerAsync(string containerName);
}