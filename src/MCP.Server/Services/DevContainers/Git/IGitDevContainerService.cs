namespace MCP.Server.Services.DevContainers.Git;

public interface IGitDevContainerService
{
    Task<string> CloneRepositoryInDevContainerAsync(
        string containerName, 
        string repository, 
        CancellationToken cancellationToken);
}