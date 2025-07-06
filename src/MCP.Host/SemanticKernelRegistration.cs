using Microsoft.SemanticKernel;
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
    }
}
