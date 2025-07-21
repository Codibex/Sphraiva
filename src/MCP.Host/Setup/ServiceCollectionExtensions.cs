using MCP.BackgroundWorker.FileSystem.Contracts;
using MCP.Host.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using OllamaSharp;
using Qdrant.Client;

namespace MCP.Host.Setup;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSemanticKernel(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(sp =>
            new OllamaApiClient(new HttpClient
            {
                BaseAddress = new Uri(configuration["OLLAMA_SERVER"] ?? throw new InvalidOperationException("Configuration key 'OLLAMA_SERVER' is missing or null.")),
                Timeout = TimeSpan.FromMinutes(20)
            }, string.IsNullOrWhiteSpace(configuration["LLM_MODEL"]) 
                ? throw new ArgumentException("The configuration value for 'LLM_MODEL' is missing or empty.") 
                : configuration["LLM_MODEL"]!)
        );

        services.AddTransient(sp =>
        {
            var ollamaClient = sp.GetRequiredService<OllamaApiClient>();
            var kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder
                .AddOllamaChatClient(ollamaClient)
                .AddOllamaChatCompletion(ollamaClient)
                .AddOllamaTextGeneration(ollamaClient)
                .AddOllamaEmbeddingGenerator(ollamaClient);
            return kernelBuilder.Build();
        });
        services.AddTransient<IKernelProvider, KernelProvider>();

        return services;
    }

    public static IServiceCollection AddQdrantServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(sp =>
        {
            var host = configuration["QDRANT_HOST"];
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new InvalidOperationException("QDRANT_HOST configuration value is missing or empty.");
            }

            var portValue = configuration["QDRANT_PORT"];
            if (!int.TryParse(portValue, out var port))
            {
                throw new InvalidOperationException("QDRANT_PORT configuration value is missing or not a valid integer.");
            }

            return new QdrantClient(host, port);
        });
        services.AddTransient<ITextSearchStringMapper, TextParagraphTextSearchStringMapper>();
        services.AddTransient<ITextSearchResultMapper, TextParagraphTextSearchResultMapper>();

        services.AddTransient(sp => sp.GetRequiredService<Kernel>().GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>());

        services.AddKeyedTransient(null, (sp, obj) =>
        {
            var stringMapper = sp.GetRequiredService<ITextSearchStringMapper>();
            var resultMapper = sp.GetRequiredService<ITextSearchResultMapper>();
            var vectorizedSearch = sp.GetKeyedService<IVectorSearchable<TextParagraph>>(null) 
                                   ?? throw new InvalidOperationException("No IVectorizedSearch<TextParagraph> registered.");
            var textEmbeddingGenerationService = sp.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
            if (vectorizedSearch is not null && textEmbeddingGenerationService is not null)
            {
                return new VectorStoreTextSearch<TextParagraph>(
                    vectorizedSearch,
                    textEmbeddingGenerationService,
                    stringMapper,
                    resultMapper);
            }
            throw new InvalidOperationException("No ITextEmbeddingGenerationService registered.");
        });

        services.AddQdrantVectorStore();
        services.AddQdrantCollection<Guid, TextParagraph>("documents");
        
        return services;
    }
}
