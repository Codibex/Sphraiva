using MCP.Host.Plugins;
using Microsoft.SemanticKernel;
using OllamaSharp;

namespace MCP.Host.Services;

public class KernelFactory(IServiceProvider services, IMcpPluginCache pluginCache) : IKernelFactory
{
    public Kernel Create(bool withPlugins = true)
    {
        var ollamaClient = services.GetRequiredService<OllamaApiClient>();
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddLogging(c => c.AddConsole().SetMinimumLevel(LogLevel.Information));
        kernelBuilder
            .AddOllamaChatClient(ollamaClient)
            .AddOllamaChatCompletion(ollamaClient)
            .AddOllamaTextGeneration(ollamaClient)
            .AddOllamaEmbeddingGenerator(ollamaClient);

        AddPlugins(withPlugins, kernelBuilder);

        kernelBuilder.Services.AddSingleton<IChatHistoryProvider, ChatHistoryProvider>();

        return kernelBuilder.Build();
    }

    private void AddPlugins(bool withPlugins, IKernelBuilder kernelBuilder)
    {
        if (!withPlugins)
        {
            return;
        }

        var tools = pluginCache.GetToolsForPlugin(PluginDescriptions.SphraivaPlugin.NAME);
        kernelBuilder.Plugins.AddFromFunctions(PluginDescriptions.SphraivaPlugin.NAME,
            tools.Select(t => t.AsKernelFunction()));
    }
}