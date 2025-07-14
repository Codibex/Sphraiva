using Microsoft.Extensions.DependencyInjection;
using MCP.Server.Services.DevContainers;
using MCP.Server.Services.FileSystem;
using Docker.DotNet;

namespace MCP.Server.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDevContainerServices(this IServiceCollection services)
    {
        services
            .AddScoped<IDevContainerService, DevContainerService>()
            .AddScoped<IDevContainerBuilder, DevContainerBuilder>()
            .AddScoped<IDevContainerCreator, DevContainerCreator>()
            .AddScoped<IDockerTarService, DockerTarService>()
            .AddTransient(_ => new DockerClientConfiguration().CreateClient());
        return services;
    }

    public static IServiceCollection AddFileSystemServices(this IServiceCollection services)
    {
        services.AddScoped<IFileSystemService, FileSystemService>();
        return services;
    }
}
