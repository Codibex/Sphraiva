using MCP.Server.Services.DevContainers;
using System.Text.RegularExpressions;

namespace MCP.Server.Services.Git;

public class GitDevContainerService(IDevContainerService devContainerService) : IGitDevContainerService
{
    private const int MAX_COMMIT_LENGTH = 2048;
    
    public async Task<string> CloneRepositoryInDevContainerAsync(string containerName, string repository, CancellationToken cancellationToken)
    {
        var safeRepo = SanitizeInput(repository);
        var command = $"gh repo clone {safeRepo}";
        return await devContainerService.RunCommandInContainerAsync(containerName, command, cancellationToken);
    }

    public async Task<string> CheckoutBranchInDevContainerAsync(string containerName, string repository, string branchName, CancellationToken cancellationToken)
    {
        var safeRepo = SanitizeInput(repository);
        var safeBranch = SanitizeInput(branchName);
        var repoName = safeRepo.Contains('/')
            ? safeRepo[(safeRepo.LastIndexOf('/') + 1)..]
            : safeRepo;
        var commands = $"cd {repoName} && " +
                       "git fetch origin main && " +
                       "git checkout main && " +
                       "git pull origin main && " +
                       $"git checkout -b {safeBranch}";
        return await devContainerService.RunCommandInContainerAsync(containerName, commands, cancellationToken);
    }

    public async Task<string> CommitChangesInDevContainerAsync(string containerName, string repository, string commitMessage, CancellationToken cancellationToken)
    {
        var safeRepo = SanitizeInput(repository);
        var repoName = safeRepo.Contains('/')
            ? safeRepo[(safeRepo.LastIndexOf('/') + 1)..]
            : safeRepo;
        var safeMessage = SanitizeCommitMessage(commitMessage);
        var command = $"cd {repoName} && " +
                      "git add . && " +
                      $"git commit -m \"{safeMessage}\"";
        return await devContainerService.RunCommandInContainerAsync(containerName, command, cancellationToken);
    }

    public async Task<string> PushBranchInDevContainerAsync(string containerName, string repository, string branchName, CancellationToken cancellationToken)
    {
        var safeRepo = SanitizeInput(repository);
        var safeBranch = SanitizeInput(branchName);
        var repoName = safeRepo.Contains('/')
            ? safeRepo[(safeRepo.LastIndexOf('/') + 1)..]
            : safeRepo;
        var command = $"cd {repoName} && git push -u origin {safeBranch}";
        return await devContainerService.RunCommandInContainerAsync(containerName, command, cancellationToken);
    }

    private static string SanitizeInput(string input)
    {
        return Regex.Replace(input, @"[^a-zA-Z0-9\-_/\.]+", "");
    }

    private static string SanitizeCommitMessage(string input)
    {
        var sanitized = Regex.Replace(input, "[\r\n\"`$&|;]", " ");
        return sanitized.Length > MAX_COMMIT_LENGTH ? sanitized[..MAX_COMMIT_LENGTH] : sanitized;
    }
}