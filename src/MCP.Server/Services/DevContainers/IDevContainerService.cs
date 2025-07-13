namespace MCP.Server.Services.DevContainers;

public interface IDevContainerService
{
    Task<string> CreateDevContainerAsync(string instructionName);
    Task<string> CleanupDevContainerAsync(string containerName);
    Task<string> RunCommandInContainerAsync(string containerName, string command, CancellationToken cancellationToken);
}