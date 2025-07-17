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
    [McpServerTool(Title = "Create development container", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    public async Task<string> CreateDevContainerAsync(string instructionName) 
        => await devContainerService.CreateDevContainerAsync(instructionName);

    [McpServerTool(Title = "Cleanup development container", Destructive = true, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    public async Task<string> CleanupDevContainerAsync(string containerName) 
        => await devContainerService.CleanupDevContainerAsync(containerName);

    [McpServerTool(Title = "Run command in development container", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    public async Task<string> RunCommandInDevContainerAsync(
        string containerName,
        string command, 
        CancellationToken cancellationToken) 
        => await devContainerService.RunCommandInContainerAsync(containerName, command, cancellationToken);
}
