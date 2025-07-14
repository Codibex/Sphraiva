using MCP.Server.Settings;

namespace MCP.Server.Services.DevContainers;

public interface IDockerTarService
{
    string CreateDockerTar(DockerImage dockerImage);
}