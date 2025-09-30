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
    [McpServerTool(Title = "Create a docker development container", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    public async Task<string> CreateDevContainerAsync([Description("Specifies a custom alias for the development container. This alias is user-defined and does not correspond to any Docker image name.")] string imageAlias,
        CancellationToken cancellationToken)
    {
        var result = await devContainerService.CreateDevContainerAsync(imageAlias, cancellationToken);
        return result.started
            ? $"Started container successfully: {result.containerName}"
            : $"Failed to start container: {result.containerName}.";
    }

    [McpServerTool(Title = "Cleanup a docker development container", Destructive = true, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    public async Task<string> CleanupDevContainerAsync([Description("Specifies the name of the Docker development container to be removed.")] string containerName,
        CancellationToken cancellationToken) 
        => await devContainerService.CleanupDevContainerAsync(containerName, cancellationToken);

    [McpServerTool(Title = "Run command in development container", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    public async Task<string> RunCommandInDevContainerAsync(
        [Description("Specifies the name of the Docker development container.")] string containerName,
        [Description("Specifies the command to execute inside the Docker development container.")] string command, 
        CancellationToken cancellationToken) 
        => await devContainerService.RunCommandInContainerAsync(containerName, command, cancellationToken);
}
