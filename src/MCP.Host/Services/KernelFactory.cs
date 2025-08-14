using MCP.Host.Agents.CodingAgent.Steps;
using MCP.Host.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
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

        AddPlugins(true, kernelBuilder);

        kernelBuilder.Services.AddKeyedSingleton(ManagerAgentStep.REDUCER_SERVICE_KEY, SetupReducer(kernelBuilder.Build(), ManagerSummaryInstructions));
        kernelBuilder.Services.AddKeyedSingleton(AgentGroupChatStep.REDUCER_SERVICE_KEY, SetupReducer(kernelBuilder.Build(), SuggestionSummaryInstructions));

        kernelBuilder.Services.AddKeyedSingleton(ManagerAgentStep.AGENT_SERVICE_KEY, managerAgent);
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

    private static ChatHistorySummarizationReducer SetupReducer(Kernel kernel, string instructions) =>
        new(kernel.GetRequiredService<IChatCompletionService>(), 1)
        {
            SummarizationInstructions = instructions
        };

    private const string ManagerSummaryInstructions =
        """
        Summarize the most recent user request in first person command form.
        """;

    private const string SuggestionSummaryInstructions =
        """
        Address the user directly with a summary of the response.
        """;
}