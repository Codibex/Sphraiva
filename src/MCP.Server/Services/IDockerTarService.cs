namespace MCP.Server.Services;

public interface IDockerTarService
{
    string CreateDockerTar(string dockerfilePath, string dockerTarFileName, string dockerfileName);
}