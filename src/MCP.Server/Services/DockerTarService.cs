using SharpCompress.Archives;
using SharpCompress.Archives.Tar;

namespace MCP.Server.Services;

public class DockerTarService : IDockerTarService
{
    public string CreateDockerTar(string dockerFilePath, string dockerTarFileName, string dockerfileName, params string[] additionalFilePaths)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "DockerImages");
        Directory.CreateDirectory(tempDir);

        var tarPath = Path.Combine(tempDir, dockerTarFileName);

        using var saveStream = new FileStream(tarPath, FileMode.Create, FileAccess.Write, FileShare.None);
        using var tarArchive = TarArchive.Create();
        
        tarArchive.AddEntry(dockerfileName, dockerFilePath);

        foreach (var additionalFilePath in additionalFilePaths)
        {
            var fileName = Path.GetFileName(additionalFilePath);
            tarArchive.AddEntry(fileName, additionalFilePath);
        }

        tarArchive.SaveTo(saveStream, SharpCompress.Common.CompressionType.None);

        return tarPath;
    }
}