using MCP.Server.Services.Git;

namespace MCP.Server.Endpoints;

public static class GitDevContainersEndpoints
{
    public record CloneGitRepositoryInDevContainersRequest(string RepositoryName);

    public static void MapGitDevContainerEndpoints(this IEndpointRouteBuilder endpointGroupBuilder)
    {
        var group = endpointGroupBuilder
            .MapGroup("/dev-containers/{containerName}")
            .WithTags("Git for dev containers");

        group.MapPost("/repositories",
        async (string containerName, CloneGitRepositoryInDevContainersRequest request, IGitDevContainerService gitDevContainerService, CancellationToken cancellationToken) =>
        {
            var result = await gitDevContainerService.CloneRepositoryInDevContainerAsync(containerName, request.RepositoryName, cancellationToken);
            return Microsoft.AspNetCore.Http.Results.Ok(new
            {
                result
            });
        });
    }
}
