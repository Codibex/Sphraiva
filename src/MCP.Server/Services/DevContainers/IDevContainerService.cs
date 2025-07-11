using MCP.Server.Common;

namespace MCP.Server.Services.DevContainers;

public interface IDevContainerService
{
    Task<OperationResult<string>> CreateDevContainerAsync(string instructionName);
    Task<OperationResult> CleanupDevContainerAsync(string containerName);
}