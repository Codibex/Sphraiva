using MCP.Host.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using OllamaSharp;

namespace MCP.Host.Services;

public class KernelFactory(IServiceProvider services, IMcpPluginCache pluginCache) : IKernelFactory
{
    public Kernel Create(bool withPlugins = true)
    {
        var ollamaClient = services.GetRequiredService<OllamaApiClient>();
        var kernelBuilder = CreateKernelBuilder();

        kernelBuilder
            //.AddOllamaChatClient(ollamaClient)
            .AddOllamaChatCompletion(ollamaClient)
            //.AddOllamaTextGeneration(ollamaClient)
            //.AddOllamaEmbeddingGenerator(ollamaClient)
            ;

        AddPlugins(withPlugins, kernelBuilder);

        return kernelBuilder.Build();
    }

    public Kernel CreateAgentGroupChatKernel(ChatCompletionAgent managerAgent, AgentGroupChat chat)
    {
        var ollamaClient = services.GetRequiredService<OllamaApiClient>();
        var kernelBuilder = CreateKernelBuilder();

        kernelBuilder
            .AddOllamaChatCompletion(ollamaClient);

        kernelBuilder.Services.AddKeyedSingleton("ManagerKey", managerAgent);
        kernelBuilder.Services.AddSingleton(chat);

        return kernelBuilder.Build();
    }

    private static IKernelBuilder CreateKernelBuilder()
    {
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddLogging(c => c.AddConsole().SetMinimumLevel(LogLevel.Information));
        kernelBuilder.Services.AddSingleton<IChatHistoryProvider, ChatHistoryProvider>();
        return kernelBuilder;
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