using Docker.DotNet;
using Docker.DotNet.Models;
using MCP.Server.Settings;

namespace MCP.Server.Services.DevContainers;

public class DevContainerBuilder(
    IDockerTarService dockerTarService, 
    DockerClient dockerClient) : IDevContainerBuilder
{
    private const string DOCKER_FILE_NAME = "DockerFile";

    public async Task BuildAsync(DockerImage dockerImage)
    {
        var tarFilePath = dockerTarService.CreateDockerTar(dockerImage);

        await using var fs = File.OpenRead(tarFilePath);

        var buildParams = new ImageBuildParameters
        {
            Dockerfile = DOCKER_FILE_NAME,
            Tags = [$"{dockerImage.ImageName}:latest"]
        };
        await dockerClient.Images.BuildImageFromDockerfileAsync(
            buildParams,
            fs,
            null,
            null,
            new Progress<JSONMessage>()
        );
    }
}