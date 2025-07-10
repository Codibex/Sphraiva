using Docker.DotNet;
using Docker.DotNet.Models;
using MCP.Server.Common;
using MCP.Server.Settings;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace MCP.Server.Services;

public class DevContainerService(IOptions<DevContainerSettings> options) : IDevContainerService
{
    private const string CONTAINER_BASE_NAME = "agent-dev-";
    private const string IMAGE_BASE_NAME = "agent-dev-img-";

    private readonly DevContainerSettings _settings = options.Value;

    public async Task<OperationResult<string>> CreateDevContainerAsync()
    {
        var path = Path.Combine(_settings.DataDirectory, _settings.DevContainerImageName);
        if (!File.Exists(path))
        {
            return OperationResult<string>.Failure("File not found!");
        }

        var dockerfilePath = await CopyDockerFile(path);
        var imageToUse = IMAGE_BASE_NAME + _settings.DevContainerImageName;
        var containerName = CreateContainerName();
        using var client = CreateDockerClient();

        var imageTag = imageToUse.Replace("_DockerFile", "");
        var images = await client.Images.ListImagesAsync(new ImagesListParameters { All = true });
        var imageExists = images.Any(img => (img.RepoTags != null) && img.RepoTags.Contains(imageTag + ":latest"));
        if (!imageExists)
        {
            await BuildImage(client, dockerfilePath, imageTag);
        }

        var response = await client.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = imageToUse,
            Name = containerName,
            Tty = false
        });

        var started = await client.Containers.StartContainerAsync(response.ID, null);
        return started 
            ? OperationResult<string>.Success(containerName)
            : OperationResult<string>.Failure($"Failed to start container {containerName}.");
    }

    public async Task<OperationResult> CleanupDevContainerAsync(string containerName)
    {
        using var client = CreateDockerClient();
        var containers = await client.Containers.ListContainersAsync(new ContainersListParameters { All = true });
        var container = containers.FirstOrDefault(c => c.Names.Contains($"/{containerName}"));
        if (container == null)
        {
            return OperationResult.Success();
        }
        if (container.State == "running")
        {
            await client.Containers.StopContainerAsync(container.ID, new ContainerStopParameters());
        }
        await client.Containers.RemoveContainerAsync(container.ID, new ContainerRemoveParameters { Force = true });

        return OperationResult.Success();
    }

    private static async Task<string> CopyDockerFile(string path)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "DockerImages");
        Directory.CreateDirectory(tempDir);

        var dockerfilePath = Path.Combine(tempDir, "Dockerfile");

        var dockerFileContent = await File.ReadAllTextAsync(path);
        await File.WriteAllTextAsync(dockerfilePath, dockerFileContent);

        return dockerfilePath;
    }

    private static async Task BuildImage(DockerClient client, string dockerfilePath, string imageTag)
    {
        var fs = File.OpenRead(dockerfilePath);

        var buildParams = new ImageBuildParameters { Dockerfile = "Dockerfile", Tags = [imageTag] };
        await client.Images.BuildImageFromDockerfileAsync(
            buildParams,
            fs,
            null,
            null,
            new Progress<JSONMessage>()
        );
    }

    private static DockerClient CreateDockerClient()
        //=> new DockerClientConfiguration(new Uri(@"unix:///var/run/docker.sock")).CreateClient();
        => new DockerClientConfiguration().CreateClient();

    private static string CreateContainerName()
    {
        var guid = Guid.NewGuid().ToString();
        var shortHash = GetShortHash(guid, 10);
        return $"{CONTAINER_BASE_NAME}{shortHash}";
    }

    /// <summary>
    /// Generates a short hash from a string.
    /// </summary>
    private static string GetShortHash(string input, int length)
    {
        using var sha256 = SHA256.Create();
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder();
        foreach (var b in hash)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString()[..length];
    }
}