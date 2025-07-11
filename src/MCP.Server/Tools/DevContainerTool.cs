using ModelContextProtocol.Server;
using MCP.Server.Services;
using System.ComponentModel;

namespace MCP.Server.Tools;

[McpServerToolType]
[Description(
    """
    Exposes server-side Docker container operations for agents or tools to create and remove development containers. " +
    "Use these tools to programmatically manage isolated development environments as part of automated workflows, agent actions, or user requests. " +
    "All container names are generated and managed by the server for security and consistency reasons."
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
    public async Task<CreateDevContainerResult> CreateDevContainerAsync(
        [Description("The Git user name to configure in the container")]
        string gitUserName,
        [Description("The Git user email to configure in the container")]
        string gitUserEmail
    )
    {
        var result = await devContainerService.CreateDevContainerAsync(new GitConfig(gitUserName, gitUserEmail));
        return result.IsSuccess
            ? CreateDevContainerResult.SuccessResult(result.Data)
            : CreateDevContainerResult.FailureResult(result.ErrorMessage!);
    }

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
    public async Task<CleanupDevContainerResult> CleanupDevContainerAsync(
        [Description("The name of the container to remove (must be a valid agent-dev container name)")]
        string containerName
    )
    {
        var result = await devContainerService.CleanupDevContainerAsync(containerName);
        return result.IsSuccess
            ? CleanupDevContainerResult.SuccessResult("Dev container removed")
            : CleanupDevContainerResult.FailureResult(result.ErrorMessage!);
    }
}

public record CreateDevContainerResult(bool IsError, string? Content, string? ErrorMessage) : ResultBase<string?>(IsError, Content, ErrorMessage)
{
    public static CreateDevContainerResult SuccessResult(string? content) => new(false, content, null);
    public static CreateDevContainerResult FailureResult(string errorMessage) => new(true, null, errorMessage);
}

public record CleanupDevContainerResult(bool IsError, string? Content, string? ErrorMessage) : ResultBase<string?>(IsError, Content, ErrorMessage)
{
    public static CleanupDevContainerResult SuccessResult(string? content) => new(false, content, null);
    public static CleanupDevContainerResult FailureResult(string errorMessage) => new(true, null, errorMessage);
}

