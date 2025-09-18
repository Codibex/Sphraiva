using MCP.Server.Services.DevContainers;

namespace MCP.Server.Endpoints;

public static class DevContainersEndpoints
{
    public record CreateDevContainersRequest(string dockerImageAlias);

    public static void MapDevContainerEndpoints(this IEndpointRouteBuilder endpointGroupBuilder)
    {
        var group = endpointGroupBuilder
            .MapGroup("/dev-containers")
            .WithTags("Dev Containers");
        group.MapPost("/",
        async (CreateDevContainersRequest request, IDevContainerService devContainerService, CancellationToken cancellationToken) =>
        {
            var result = await devContainerService.CreateDevContainerAsync(request.dockerImageAlias, cancellationToken);
            return result.started 
            ? Microsoft.AspNetCore.Http.Results.Ok(new
                {
                    result.containerName
                })
                : Microsoft.AspNetCore.Http.Results.BadRequest(new
                {
                    Error = $"Failed to start container: {result.containerName}."
                });
        });
    }
}
