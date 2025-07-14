using MCP.Server.Settings;
using Microsoft.Extensions.Options;
using SharpCompress.Archives;
using SharpCompress.Archives.Tar;

namespace MCP.Server.Services.DevContainers;

public class DockerTarService(IOptions<DevContainerSettings> options) : IDockerTarService
{
    private readonly DevContainerSettings _settings = options.Value;

    private const string IMAGE_DIRECTORY = "dockerimages";

    public string CreateDockerTar(DockerImage dockerImage)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "DockerImages");
        Directory.CreateDirectory(tempDir);

        var tarFilePath = Path.Combine(tempDir, $"{dockerImage.ImageName}.tar");

        using var saveStream = new FileStream(tarFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
        using var tarArchive = TarArchive.Create();

        var dockerImageDirectory = Path.Combine(_settings.DataDirectory, IMAGE_DIRECTORY, dockerImage.Path);
        var files = Directory.GetFiles(dockerImageDirectory);

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            tarArchive.AddEntry(fileName, file);
        }

        tarArchive.SaveTo(saveStream, SharpCompress.Common.CompressionType.None);

        return tarFilePath;
    }
}