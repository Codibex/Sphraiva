using Docker.DotNet;
using Docker.DotNet.Models;
using MCP.Server.Common;
using MCP.Server.Settings;
using Microsoft.Extensions.Options;

namespace MCP.Server.Services.DevContainers;

public class DevContainerService(
    IOptions<DevContainerSettings> options,
    DockerClient dockerClient,
    IDevContainerBuilder devContainerBuilder,
    IDevContainerCreator devContainerCreator) : IDevContainerService
{
    private readonly DevContainerSettings _settings = options.Value;

    public async Task<OperationResult<string>> CreateDevContainerAsync(string instructionName)
    {
        if (string.IsNullOrWhiteSpace(instructionName))
        {
            return OperationResult<string>.Failure("Instruction name cannot be empty.");
        }

        var dockerImage = _settings.GetImageByInstructionName(instructionName);
        if (dockerImage == null)
        {
            return OperationResult<string>.Failure($"No Docker image found for instruction '{instructionName}'.");
        }

        var images = await dockerClient.Images.ListImagesAsync(new ImagesListParameters { All = true });

        var imageExists = images.Any(img => img.RepoTags != null && img.RepoTags.Contains(dockerImage + ":latest"));
        if (!imageExists)
        {
            await devContainerBuilder.BuildAsync(dockerImage);
        }

        var result = await devContainerCreator.CreateAsync(dockerImage);

        var started = await dockerClient.Containers.StartContainerAsync(result.Id, null);
        return started
            ? OperationResult<string>.Success(result.ContainerName)
            : OperationResult<string>.Failure($"Failed to start container {result.ContainerName}.");
    }

    public async Task<OperationResult> CleanupDevContainerAsync(string containerName)
    {
        var container = await FindContainer(containerName);
        if (container == null)
        {
            return OperationResult.Success();
        }
        if (container.State == "running")
        {
            await dockerClient.Containers.StopContainerAsync(container.ID, new ContainerStopParameters());
        }
        await dockerClient.Containers.RemoveContainerAsync(container.ID, new ContainerRemoveParameters { Force = true });

        return OperationResult.Success();
    }

    public async Task<OperationResult<string>> RunCommandInContainerAsync(string containerName, string command, CancellationToken cancellationToken)
    {
        var container = await FindContainer(containerName);
        if (container == null)
        {
            return OperationResult<string>.Failure($"Container '{containerName}' not found.");
        }

        container.Command = command;

        var parameter = new ContainerExecCreateParameters
        {
            AttachStderr = true,
            AttachStdout = true,
            Cmd = ["sh", "-c", command]
        };

        var exec = await dockerClient.Exec.ExecCreateContainerAsync(container.ID, parameter, cancellationToken);
        var stream = await dockerClient.Exec.StartAndAttachContainerExecAsync(exec.ID, false, cancellationToken);

        var output = await stream.ReadOutputToEndAsync(cancellationToken);

        return OperationResult<string>.Success(output.stdout!);
    }

    private async Task<ContainerListResponse?> FindContainer(string containerName)
    {
        var containers = await dockerClient.Containers.ListContainersAsync(new ContainersListParameters { All = true });
        var container = containers.FirstOrDefault(c => c.Names.Contains($"/{containerName}"));
        return container;
    }
}