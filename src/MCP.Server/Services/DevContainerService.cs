using Docker.DotNet;
using Docker.DotNet.Models;
using MCP.Server.Common;
using MCP.Server.Settings;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace MCP.Server.Services;

public record GitConfig(string UserName, string UserEmail)
{
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(UserEmail))
        {
            return false;
        }
        return true;
    }
};

public class DevContainerService(
    IOptions<DevContainerSettings> options,
    IDockerTarService dockerTarService) : IDevContainerService
{
    private const string DOCKER_TAR_FILE_NAME = "docker.tar";
    private const string DOCKER_FILE_NAME = "DockerFile";
    private const string CONTAINER_BASE_NAME = "agent-dev-";
    private const string IMAGE_BASE_NAME = "agent-dev-";

    private readonly DevContainerSettings _settings = options.Value;

    public async Task<OperationResult<string>> CreateDevContainerAsync(GitConfig gitConfig)
    {
        if(!gitConfig.IsValid())
        {
            return OperationResult<string>.Failure("Git user name and email are required.");
        }

        var dockerFilePath = Path.Combine(_settings.DataDirectory, _settings.DevContainerImageName);
        if (!File.Exists(dockerFilePath))
        {
            return OperationResult<string>.Failure("File not found!");
        }

        var entrypointFilePath = Path.Combine(_settings.DataDirectory, "entrypoint.sh");

        var dockerfilePath = dockerTarService.CreateDockerTar(dockerFilePath, DOCKER_TAR_FILE_NAME, DOCKER_FILE_NAME, entrypointFilePath);
        var imageToUse = IMAGE_BASE_NAME + _settings.DevContainerImageName;
        var containerName = CreateContainerName();
        using var client = CreateDockerClient();

        var imageTag = imageToUse.Replace($"_{DOCKER_FILE_NAME}", "");
        var images = await client.Images.ListImagesAsync(new ImagesListParameters { All = true });
        var imageExists = images.Any(img => (img.RepoTags != null) && img.RepoTags.Contains(imageTag + ":latest"));
        if (!imageExists)
        {
            await BuildImage(client, dockerfilePath, imageTag);
        }

        var envVars = new List<string>
        {
            $"GIT_USER_NAME={gitConfig.UserName}",
            $"GIT_USER_EMAIL={gitConfig.UserEmail}"
        };

        var hostConfig = new HostConfig();
        if (_settings.VolumeBinds is not null && _settings.VolumeBinds.Count > 0)
        {
           hostConfig.Binds = _settings.VolumeBinds.ToList();
        }

        var response = await client.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = imageTag,
            Name = containerName,
            Tty = false,
            Env = envVars,
            HostConfig = hostConfig.Binds != null ? hostConfig : null
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

    private static async Task BuildImage(DockerClient client, string dockerfilePath, string imageTag)
    {
        await using var fs = File.OpenRead(dockerfilePath);

        var buildParams = new ImageBuildParameters
        {
            Dockerfile = DOCKER_FILE_NAME,
            Tags = [$"{imageTag}:latest"]
        };
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