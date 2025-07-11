namespace MCP.Server.Services;

public interface IDockerTarService
{
    string CreateDockerTar(string dockerFilePath, string dockerTarFileName, string dockerfileName, params string[] additionalFilePaths);
}