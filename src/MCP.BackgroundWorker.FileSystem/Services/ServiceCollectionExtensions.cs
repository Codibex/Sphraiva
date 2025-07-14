using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;

namespace MCP.BackgroundWorker.FileSystem.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackgroundWorkerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<DataUploader>();
        services.AddQdrantVectorStore(configuration["QDRANT_HOST"]!, int.Parse(configuration["QDRANT_PORT"]!), false);
        services.AddSingleton(sp => sp.GetRequiredService<Kernel>().GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>());
        services.AddHostedService<Worker>();
        return services;
    }

    
}
