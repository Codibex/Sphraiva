using MCP.Server.Settings;

namespace MCP.Server.Services.DevContainers;

public interface IDevContainerCreator
{
    Task<DevContainerCreationResult> CreateAsync(DockerImage dockerImage);
}