using SharpCompress.Archives.Tar;

namespace MCP.Server.Services;

public class DockerTarService : IDockerTarService
{
    public string CreateDockerTar(string dockerfilePath, string dockerTarFileName, string dockerfileName)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "DockerImages");
        Directory.CreateDirectory(tempDir);

        var tarPath = Path.Combine(tempDir, dockerTarFileName);

        using var saveStream = new FileStream(tarPath, FileMode.Create, FileAccess.Write, FileShare.None);
        using var tarArchive = TarArchive.Create();
        
        using var dockerFileStream = new FileStream(dockerfilePath, FileMode.Open);
        tarArchive.AddEntry(dockerfileName, dockerFileStream, true);

        tarArchive.SaveTo(saveStream, SharpCompress.Common.CompressionType.None);

        return tarPath;
    }
}