using Microsoft.SemanticKernel;
using OllamaSharp;

namespace MCP.BackgroundWorker.FileSystem.Setup;
internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddSemantikKernel(this IServiceCollection services, OllamaApiClient ollamaClient)
    {
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddOllamaEmbeddingGenerator(ollamaClient);
        var kernel = kernelBuilder.Build();
        services.AddSingleton(kernel);

        return services;
    }

    internal static OllamaApiClient RegisterOllamaClient(this IServiceCollection services, IConfiguration configuration)
    {
        var ollamaClient = new OllamaApiClient(configuration["OLLAMA_SERVER"]!, configuration["LLM_MODEL"]!);
        services.AddSingleton(ollamaClient);
        return ollamaClient;
    }
}
