namespace MCP.Server.Services.Git;

public interface IGitDevContainerService
{
    Task<string> CloneRepositoryInDevContainerAsync(
        string containerName, 
        string repository, 
        CancellationToken cancellationToken);

    Task<string> CheckoutBranchInDevContainerAsync(
        string containerName,
        string repository,
        string branchName,
        CancellationToken cancellationToken);
}