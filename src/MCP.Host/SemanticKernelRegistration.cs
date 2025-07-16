using MCP.Host.Data;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using ModelContextProtocol.Client;
using OllamaSharp;
using Qdrant.Client;

namespace MCP.Host;

public static class SemanticKernelRegistration
{
    public static void AddSemanticKernel(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<OllamaApiClient>(sp =>
            new OllamaApiClient(new HttpClient
            {
                BaseAddress = new Uri(configuration["OLLAMA_SERVER"]!),
                Timeout = TimeSpan.FromMinutes(5)
            }, configuration["LLM_MODEL"]!)
        );

        services.AddSingleton<QdrantClient>(sp =>
            new QdrantClient(configuration["QDRANT_HOST"]!, int.Parse(configuration["QDRANT_PORT"]!))
        );

        services.AddTransient<ITextSearchStringMapper, TextParagraphTextSearchStringMapper>();
        services.AddTransient<ITextSearchResultMapper, TextParagraphTextSearchResultMapper>();

        services.AddTransient<Kernel>(sp =>
        {
            var ollamaClient = sp.GetRequiredService<OllamaApiClient>();
            var kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder
                .AddOllamaChatClient(ollamaClient)
                .AddOllamaChatCompletion(ollamaClient)
                .AddOllamaTextGeneration(ollamaClient)
                .AddOllamaEmbeddingGenerator(ollamaClient);
            var kernel = kernelBuilder.Build();
            return kernel;
        });

        services.AddTransient<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
            sp.GetRequiredService<Kernel>().GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>());
        services.AddQdrantVectorStore();

        services.AddKeyedTransient<VectorStoreTextSearch<TextParagraph>>(null, (sp, obj) =>
        {
            var stringMapper = sp.GetRequiredService<ITextSearchStringMapper>();
            var resultMapper = sp.GetRequiredService<ITextSearchResultMapper>();
            var vectorizedSearch = sp.GetKeyedService<IVectorSearchable<TextParagraph>>(null);
            if (vectorizedSearch is null)
            {
                throw new InvalidOperationException("No IVectorizedSearch<TextParagraph> registered.");
            }
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

        services.AddQdrantCollection<Guid, TextParagraph>("documents");
    }

    private static async Task RegisterMcp(Kernel kernel)
    {
        await Task.Delay(5000);
        var mcpClient = await McpClientFactory.CreateAsync(new SseClientTransport(new SseClientTransportOptions()
        {
            Endpoint = new Uri("http://sphraiva-mcp-server:8080/"),
            TransportMode = HttpTransportMode.AutoDetect
        }));

        var tools = await mcpClient.ListToolsAsync().ConfigureAwait(false);
        var resources = await mcpClient.ListResourcesAsync().ConfigureAwait(false);
        var templateResources = await mcpClient.ListResourceTemplatesAsync().ConfigureAwait(false);

        kernel.Plugins.AddFromFunctions("Sphraiva", tools.Select(t => t.AsKernelFunction()));
    }
}

/// <summary>
/// String mapper which converts a DataModel to a string.
/// </summary>
internal sealed class TextParagraphTextSearchStringMapper : ITextSearchStringMapper
{
    /// <inheritdoc />
    public string MapFromResultToString(object result)
    {
        if (result is TextParagraph dataModel)
        {
            return dataModel.Text;
        }
        throw new ArgumentException("Invalid result type.");
    }
}

/// <summary>
/// Result mapper which converts a DataModel to a TextSearchResult.
/// </summary>
internal sealed class TextParagraphTextSearchResultMapper : ITextSearchResultMapper
{
    /// <inheritdoc />
    public TextSearchResult MapFromResultToTextSearchResult(object result)
    {
        if (result is TextParagraph dataModel)
        {
            return new TextSearchResult(value: dataModel.Text)
            {
                Name = dataModel.DocumentUri, 
                Link = dataModel.DocumentUri,
            };
        }
        throw new ArgumentException("Invalid result type.");
    }
}