using MCP.Server.Settings;

namespace MCP.Server.Services.DevContainers;

public interface IDevContainerBuilder
{
    Task BuildAsync(DockerImage dockerImage);
}