using MCP.Server.Services.Git;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MCP.Server.Tools;

[McpServerToolType]
[Description(
    """
    Provides tools to execute common Git commands inside Docker development containers. 
    Enables agents and tools to perform repository operations such as clone, checkout, commit, and push within a specified dev container.
    """
)]
public class GitDevContainerTool(IGitDevContainerService gitDevContainerService)
{
    [McpServerTool(Title = "Clone a Github repository into a Docker development container.", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    public async Task<string> CloneRepositoryInDevContainerAsync(string containerName, string repository, CancellationToken cancellationToken)
        => await gitDevContainerService.CloneRepositoryInDevContainerAsync(containerName, repository, cancellationToken);

    [McpServerTool(Title = "Checkout a branch based on the main branch in a Git repository inside a Docker development container.", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    public async Task<string> CheckoutBranchInDevContainerAsync(string containerName, string repository, string branchName, CancellationToken cancellationToken)
        => await gitDevContainerService.CheckoutBranchInDevContainerAsync(containerName, repository, branchName, cancellationToken);

    [McpServerTool(Title = "Commit changes to a Git repository inside a Docker development container.", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    public async Task<string> CommitChangesInDevContainerAsync(string containerName, string repository, string commitMessage, CancellationToken cancellationToken)
        => await gitDevContainerService.CommitChangesInDevContainerAsync(containerName, repository, commitMessage, cancellationToken);

    [McpServerTool(Title = "Push a branch to a remote in a Github repository inside a Docker development container.", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    public async Task<string> PushBranchInDevContainerAsync(
        string containerName,
        string repository,
        string branchName,
        CancellationToken cancellationToken)
        => await gitDevContainerService.PushBranchInDevContainerAsync(containerName, repository, branchName, cancellationToken);
}
