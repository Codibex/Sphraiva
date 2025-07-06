using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
using OllamaSharp;

namespace MCP.Host;

public static class SemanticKernelRegistration
{
    public static void AddSemanticKernel(this IServiceCollection services)
    {
        var ollamaClient = new OllamaApiClient(@"http://ollama:11434", "mistral");
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder
            .AddOllamaChatClient(ollamaClient)
            .AddOllamaChatCompletion(ollamaClient)
            .AddOllamaTextGeneration(ollamaClient)
            .AddOllamaEmbeddingGenerator(ollamaClient);

        var kernel = kernelBuilder.Build();

        var tools = GetToolsAsync().GetAwaiter().GetResult();

        
        kernel.Plugins.AddFromFunctions("Sphraiva", tools.Select(t => t.AsKernelFunction()));

        services.AddSingleton(kernel);
        services.AddSingleton(ollamaClient);
    }

    private static async Task<IList<McpClientTool>> GetToolsAsync()
    {
        await Task.Delay(5000);
        await using var mcpClient = await McpClientFactory.CreateAsync(new SseClientTransport(new SseClientTransportOptions()
        {
            Endpoint = new Uri("http://sphraiva-mcp-server:8080/"),
            TransportMode = HttpTransportMode.AutoDetect
        }));

        var tools = await mcpClient.ListToolsAsync().ConfigureAwait(false);
        return tools;
    }
}
