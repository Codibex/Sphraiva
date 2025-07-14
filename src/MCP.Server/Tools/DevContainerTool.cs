using ModelContextProtocol.Server;
using System.ComponentModel;
using MCP.Server.Services.DevContainers;

namespace MCP.Server.Tools;

[McpServerToolType]
[Description(
    """
    Provides operations to create and remove Docker development containers for agents and tools.
    """
)]
public class DevContainerTool(IDevContainerService devContainerService)
{
    [McpServerTool(Title = "Create dev container", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    [Description(
        """
        Creates a Docker development container on the server.
        Returns the name of the created container if successful, or an error message if the operation fails.
        Sample phrases:
        - "Create a new dev container."
        - "Provision a fresh agent-dev container."
        """
    )]
    public async Task<string> CreateDevContainerAsync(
        [Description(
            """
            The instruction keyword used to select the Docker image configuration. 
            For example, 'net9' requests a .NET 9 development container. 
            """
        )]
        string instructionName
    ) => await devContainerService.CreateDevContainerAsync(instructionName);

    [McpServerTool(Title = "Cleanup dev container", Destructive = true, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    [Description(
        """
        Removes a Docker development container from the server by name.
        Stops the container if running and deletes it.
        Returns a success result if removed, or an error message if the operation fails.
        Sample phrases:
        - "Remove dev container agent-dev-abc123."
        - "Cleanup agent-dev container by name."
        """
    )]
    public async Task<string> CleanupDevContainerAsync(
        [Description("The name of the container to remove (must be a valid agent-dev container name)")]
        string containerName
    ) => await devContainerService.CleanupDevContainerAsync(containerName);

    [McpServerTool(Title = "Run command in dev container", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    [Description(
        """
        Runs a shell command inside a Docker development container by name. Useful for operations like 'git clone', 'apt install', or custom scripts.
        Returns the output of the command or an error message if the operation fails.
        Sample phrases:
        - "Run 'git clone https://github.com/example/repo.git' in dev container agent-dev-abc123."
        - "Execute 'ls -la' in dev container agent-dev-xyz456."
        """
    )]
    public async Task<string> RunCommandInDevContainerAsync(
        [Description("The name of the container to remove (must be a valid agent-dev container name)")]
        string containerName,
        [Description("The command to be executed in the container shell.")]
        string command, 
        CancellationToken cancellationToken) 
        => await devContainerService.RunCommandInContainerAsync(containerName, command, cancellationToken);
}
