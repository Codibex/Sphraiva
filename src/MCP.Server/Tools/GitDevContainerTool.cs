using MCP.Server.Services.Git;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MCP.Server.Tools;

[McpServerToolType]
[Description(
    """
    Executes git clone operations inside a Docker development container for agents and tools.
    Use this tool to clone repositories directly into a running dev container by specifying its name.
    Returns the output of the operation or an error message if the command fails.
    Example phrases:
    - "Clone the repository 'Codibex/Sphraiva' in dev container agent-dev-abc123."
    """
)]
public class GitDevContainerTool(IGitDevContainerService gitDevContainerService)
{
    [McpServerTool(Title = "Run command in dev container", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    [Description(
        """
        Executes git clone operations inside a Docker development container for agents and tools.
        Use this tool to clone repositories directly into a running dev container by specifying its name.
        Returns the output of the operation or an error message if the command fails.
        Example phrases:
        - "Clone the repository 'Codibex/Sphraiva' in dev container agent-dev-abc123."
        """
    )]
    public async Task<string> CloneRepositoryInDevContainerAsync(
        [Description("The name of the Docker development container in which the git command should be executed (e.g. 'agent-dev-abc123').")]
        string containerName,
        [Description("The git repository to clone into the specified container (e.g. 'Codibex/Sphraiva').")]
        string repository,
        CancellationToken cancellationToken)
        => await gitDevContainerService.CloneRepositoryInDevContainerAsync(containerName, repository, cancellationToken);

    [McpServerTool(Title = "Checkout branch from main in dev container", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    [Description(
        """
        Checks out a new branch from main inside a Docker development container for agents and tools.
        Use this tool to create and checkout a branch from main in a running dev container by specifying its name and the branch name.
        Returns the output of the operation or an error message if the command fails.
        Example phrases:
        - "Checkout branch 'feature-xyz' from main in dev container agent-dev-abc123 for repository 'Codibex/Sphraiva'."
        """
    )]
    public async Task<string> CheckoutBranchInDevContainerAsync(
        [Description("The name of the Docker development container in which the git command should be executed (e.g. 'agent-dev-abc123').")]
        string containerName,
        [Description("The git repository to operate on (e.g. 'Codibex/Sphraiva').")]
        string repository,
        [Description("The name of the branch to create and checkout from main (e.g. 'feature-xyz').")]
        string branchName,
        CancellationToken cancellationToken)
        => await gitDevContainerService.CheckoutBranchInDevContainerAsync(containerName, repository, branchName, cancellationToken);

    [McpServerTool(Title = "Push branch in dev container", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    [Description(
        """
        Pushes the specified branch to the remote repository inside a Docker development container for agents and tools.
        Use this tool to push a branch from a running dev container by specifying its name, the repository, and the branch name.
        Returns the output of the operation or an error message if the command fails.
        Example phrases:
        - "Push branch 'feature-xyz' in dev container agent-dev-abc123 for repository 'Codibex/Sphraiva'."
        """
    )]
    public async Task<string> PushBranchInDevContainerAsync(
        [Description("The name of the Docker development container in which the git command should be executed (e.g. 'agent-dev-abc123').")]
        string containerName,
        [Description("The git repository to operate on (e.g. 'Codibex/Sphraiva').")]
        string repository,
        [Description("The name of the branch to push (e.g. 'feature-xyz').")]
        string branchName,
        CancellationToken cancellationToken)
        => await gitDevContainerService.PushBranchInDevContainerAsync(containerName, repository, branchName, cancellationToken);
}
