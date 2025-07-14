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
        // Assumes repo is already cloned and working directory is set
        // 1. Fetch latest main
        // 2. Create branch from main
        // 3. Checkout new branch
        // 4. Push branch to remote
        var repoName = repository.Split('/')[1];
        var commands = $"cd {repoName} && " +
                       "git fetch origin main && " +
                       "git checkout main && " +
                       "git pull origin main && " +
                       $"git checkout -b {branchName} &&";
        //var commands = $"cd {repoName} && " +
        //               "git fetch origin main && " +
        //               "git checkout main && " +
        //               "git pull origin main && " +
        //               $"git checkout -b {branchName} && " +
        //               $"git push -u origin {branchName}";
        return await devContainerService.RunCommandInContainerAsync(containerName, commands, cancellationToken);
    }
}