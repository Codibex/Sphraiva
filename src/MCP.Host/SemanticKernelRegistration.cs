using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
using OllamaSharp;

namespace MCP.Host;

public static class SemanticKernelRegistration
{
    public static void AddSemanticKernel(this IServiceCollection services)
    {
        var ollamaClient = new OllamaApiClient("http://sphraiva-ollama:11434", "devstral");
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder
            .AddOllamaChatClient(ollamaClient)
            .AddOllamaChatCompletion(ollamaClient)
            .AddOllamaTextGeneration(ollamaClient)
            .AddOllamaEmbeddingGenerator(ollamaClient);

        var kernel = kernelBuilder.Build();

        RegisterMcp(kernel).GetAwaiter().GetResult();

        services.AddSingleton(kernel);
        services.AddSingleton(ollamaClient);
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
