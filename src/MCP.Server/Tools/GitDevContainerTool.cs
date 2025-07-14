using MCP.Server.Services.DevContainers.Git;
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
}
