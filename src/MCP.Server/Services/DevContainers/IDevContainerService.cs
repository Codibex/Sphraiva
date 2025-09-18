namespace MCP.Server.Services.DevContainers;

public interface IDevContainerService
{
    Task<(bool started, string containerName)> CreateDevContainerAsync(string instructionName, CancellationToken cancellationToken);
    Task<string> CleanupDevContainerAsync(string containerName, CancellationToken cancellationToken);
    Task<string> RunCommandInContainerAsync(string containerName, string command, CancellationToken cancellationToken);
}