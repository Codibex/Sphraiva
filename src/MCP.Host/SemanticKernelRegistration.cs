using MCP.Host.Data;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
using OllamaSharp;

namespace MCP.Host;

public static class SemanticKernelRegistration
{
    public static void AddSemanticKernel(this IServiceCollection services, IConfiguration configuration)
    {
        var ollamaClient = new OllamaApiClient(configuration["OLLAMA_SERVER"]!, configuration["LLM_MODEL"]!);
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder
            .AddOllamaChatClient(ollamaClient)
            .AddOllamaChatCompletion(ollamaClient)
            .AddOllamaTextGeneration(ollamaClient)
            .AddOllamaEmbeddingGenerator(ollamaClient)
            .AddVectorStoreTextSearch<Document>();

        var kernel = kernelBuilder.Build();

        RegisterMcp(kernel).GetAwaiter().GetResult();

        services.AddSingleton(kernel);
        services.AddSingleton(ollamaClient);
        services.AddQdrantVectorStore(configuration["QDRANT_HOST"]!, int.Parse(configuration["QDRANT_PORT"]!), false);
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
