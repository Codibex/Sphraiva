using MCP.Server.Services.DevContainers;

namespace MCP.Server.Services.Git;

public class GitDevContainerService(IDevContainerService devContainerService) : IGitDevContainerService
{
    public async Task<string> CloneRepositoryInDevContainerAsync(string containerName, string repository, CancellationToken cancellationToken)
    {
        var command = $"gh repo clone {repository}";
        return await devContainerService.RunCommandInContainerAsync(containerName, command, cancellationToken);
    }

    public async Task<string> CheckoutBranchInDevContainerAsync(string containerName, string repository, string branchName, CancellationToken cancellationToken)
    {
        var repoName = repository.Split('/')[1];
        var commands = $"cd {repoName} && " +
                       "git fetch origin main && " +
                       "git checkout main && " +
                       "git pull origin main && " +
                       $"git checkout -b {branchName}";
        return await devContainerService.RunCommandInContainerAsync(containerName, commands, cancellationToken);
    }

    public async Task<string> CommitChangesInDevContainerAsync(string containerName, string repository, string commitMessage, CancellationToken cancellationToken)
    {
        var repoName = repository.Split('/')[1];
        var command = $"cd {repoName} && " + 
                      "git add . && " +
                      $"git commit -m \"{commitMessage.Replace("\"", "\\\"")}\"";
        return await devContainerService.RunCommandInContainerAsync(containerName, command, cancellationToken);
    }

    public async Task<string> PushBranchInDevContainerAsync(string containerName, string repository, string branchName, CancellationToken cancellationToken)
    {
        var repoName = repository.Split('/')[1];
        var command = $"cd {repoName} && git push -u origin {branchName}";
        return await devContainerService.RunCommandInContainerAsync(containerName, command, cancellationToken);
    }
}