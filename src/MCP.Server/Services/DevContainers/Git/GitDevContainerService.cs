namespace MCP.Server.Services.DevContainers.Git;

public class GitDevContainerService(IDevContainerService devContainerService) : IGitDevContainerService
{
    public async Task<string> CloneRepositoryInDevContainerAsync(string containerName, string repository, CancellationToken cancellationToken)
    {
        var command = $"gh repo clone {repository}";
        await devContainerService.RunCommandInContainerAsync(containerName, command, cancellationToken);
    }
}