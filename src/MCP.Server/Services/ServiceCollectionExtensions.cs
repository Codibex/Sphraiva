using MCP.Server.Services.DevContainers;
using MCP.Server.Services.FileSystem;
using Docker.DotNet;
using MCP.Server.Services.Git;

namespace MCP.Server.Services;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddDevContainerServices(this IServiceCollection services)
    {
        services
            .AddScoped<IDevContainerService, DevContainerService>()
            .AddScoped<IDevContainerBuilder, DevContainerBuilder>()
            .AddScoped<IDevContainerCreator, DevContainerCreator>()
            .AddScoped<IDockerTarService, DockerTarService>()
            .AddTransient(_ => new DockerClientConfiguration().CreateClient());
        return services;
    }

    internal static IServiceCollection AddGitServices(this IServiceCollection services)
    {
        services.AddScoped<IGitDevContainerService, GitDevContainerService>();
        return services;
    }

    internal static IServiceCollection AddFileSystemServices(this IServiceCollection services)
    {
        services.AddScoped<IFileSystemService, FileSystemService>();
        return services;
    }
}
