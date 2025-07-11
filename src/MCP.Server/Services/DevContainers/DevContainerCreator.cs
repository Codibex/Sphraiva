using System.Security.Cryptography;
using System.Text;
using Docker.DotNet;
using Docker.DotNet.Models;
using MCP.Server.Settings;
using Microsoft.Extensions.Options;

namespace MCP.Server.Services.DevContainers;

public class DevContainerCreator(
    IOptions<DevContainerSettings> options,
    DockerClient dockerClient) : IDevContainerCreator
{
    private readonly DevContainerSettings _settings = options.Value;

    private const string GITHUB_DIRECTORY = "github";
    private const string CONTAINER_BASE_NAME = "agent-dev-";

    public async Task<DevContainerCreationResult> CreateAsync(DockerImage dockerImage)
    {
        var githubPatToken = GetGithubPatToken();

        var envVars = new List<string>
        {
            $"GIT_USER_NAME={_settings.GitUserName}",
            $"GIT_USER_EMAIL={_settings.GitUserEmail}",
            $"GH_TOKEN={githubPatToken}"
        };

        var hostConfig = new HostConfig
        {
            Binds = dockerImage.VolumeBinds?.ToList()
        };

        var containerName = CreateContainerName();

        var response = await dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = dockerImage.ImageName,
            Name = containerName,
            Tty = false,
            Env = envVars,
            HostConfig = hostConfig.Binds != null ? hostConfig : null
        });

        return new DevContainerCreationResult(response.ID, containerName);
    }

    private Task<string> GetGithubPatToken()
    {
        var file = Path.Combine(_settings.DataDirectory, GITHUB_DIRECTORY, _settings.GithubPatTokenFile);
        return File.ReadAllTextAsync(file);
    }

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