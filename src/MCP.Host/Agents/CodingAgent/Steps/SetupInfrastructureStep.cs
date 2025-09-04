using MCP.Host.Agents.Steps;
using MCP.Host.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.RegularExpressions;

namespace MCP.Host.Agents.CodingAgent.Steps;

public class SetupInfrastructureStep : KernelProcessStep
{
    private const string SYSTEM_PROMPT =
        """
        You are tasked with identifying the repository folder inside the development container based on the clone result message.
        Respond with the **exact folder path only**.
        """;

    public static class OutputEvents
    {
        public const string SETUP_INFRASTRUCTURE_SUCCEEDED = nameof(SETUP_INFRASTRUCTURE_SUCCEEDED);
        public const string SETUP_INFRASTRUCTURE_FAILED = nameof(SETUP_INFRASTRUCTURE_FAILED);
    }

    [KernelFunction]
    public async Task SetupInfrastructureAsync(Kernel kernel, KernelProcessStepContext context, InputCheckResult input)
    {
        var logger = kernel.GetRequiredService<ILogger<SetupInfrastructureStep>>();
        logger.LogInformation("Setup infrastructure");

        var plugin = await GetPluginAsync(kernel, context, logger);
        if (plugin is null)
        {
            return;
        }

        var containerCreationResult = await CreateContainerAsync(context, plugin, input, logger);
        if (containerCreationResult is null)
        {
            return;
        }

        var containerName = await ParseContainerCreationResultAsync(context, containerCreationResult, logger);
        if (containerName is null)
        {
            return;
        }

        var codingProcessContext = new CodingProcessContext
        {
            ContainerName = containerName,
            RepositoryName = input.RepositoryName,
            Requirement = input.Requirement
        };

        var cloneRepositoryResult = await CloneRepositoryAsync(context, plugin, codingProcessContext, logger);
        if (cloneRepositoryResult is null)
        {
            return;
        }

        var repositoryPath = await GetRepositoryPathAsync(kernel, cloneRepositoryResult);

        await context.EmitEventAsync(OutputEvents.SETUP_INFRASTRUCTURE_SUCCEEDED, data: new
        {
            codingProcessContext.RepositoryName,
            codingProcessContext.ContainerName,
            codingProcessContext.Requirement,
            RepositoryPath = repositoryPath
        });
    }

    private static async Task<KernelPlugin?> GetPluginAsync(Kernel kernel, KernelProcessStepContext context, ILogger<SetupInfrastructureStep> logger)
    {
        if (kernel.Plugins.TryGetPlugin(PluginDescriptions.SphraivaPlugin.NAME, out var plugin))
        {
            return plugin;
        }

        logger.LogError($"{PluginDescriptions.SphraivaPlugin.NAME} plugin is not available.");
        await context.EmitEventAsync(OutputEvents.SETUP_INFRASTRUCTURE_FAILED, data: "Setup not possible.");
        return null;

    }

    private static async Task<string?> CreateContainerAsync(KernelProcessStepContext context, KernelPlugin plugin,
        InputCheckResult input, ILogger<SetupInfrastructureStep> logger)
    {
        if (plugin.TryGetFunction(PluginDescriptions.SphraivaPlugin.Functions.CREATE_DEV_CONTAINER, out var function))
        {
            var arguments = new KernelArguments
            {
                ["instructionName"] = input.InstructionName,
            };

            var rawResponse = await function.InvokeAsync(arguments);
            return rawResponse?.ToString();
        }

        logger.LogError("Function {CreateDevContainerFunction} is not available in {PluginName} plugin.", 
            PluginDescriptions.SphraivaPlugin.Functions.CREATE_DEV_CONTAINER, PluginDescriptions.SphraivaPlugin.NAME);
        await context.EmitEventAsync(OutputEvents.SETUP_INFRASTRUCTURE_FAILED, data: "Setup not possible.");
        return null;
    }

    private static async Task<string?> ParseContainerCreationResultAsync(KernelProcessStepContext context,
        string containerCreationResult, ILogger<SetupInfrastructureStep> logger)
    {
        var match = Regex.Match(containerCreationResult, @"Started container successfully: (\S+)");
        if (match.Success)
        {
            return match.Groups[1].Value.Trim('"');
        }

        logger.LogError("Failed to parse container creation result: {containerCreationResult}",
            containerCreationResult);
        await context.EmitEventAsync(OutputEvents.SETUP_INFRASTRUCTURE_FAILED,
            data: "Failed to parse container creation result.");
        return null;
    }

    private static async Task<string?> CloneRepositoryAsync(KernelProcessStepContext context, KernelPlugin plugin, CodingProcessContext codingProcessContext, 
        ILogger<SetupInfrastructureStep> logger)
    {
        if (plugin.TryGetFunction(PluginDescriptions.SphraivaPlugin.Functions.CLONE_REPOSITORY_IN_DEV_CONTAINER, out var function))
        {
            var arguments = new KernelArguments
            {
                ["containerName"] = codingProcessContext.ContainerName,
                ["repositoryName"] = codingProcessContext.RepositoryName,
            };

            var rawResponse = await function.InvokeAsync(arguments);
            return rawResponse?.ToString();
        }

        logger.LogError("Function {CloneRepositoryFunction} is not available in {PluginName} plugin.", 
            PluginDescriptions.SphraivaPlugin.Functions.CLONE_REPOSITORY_IN_DEV_CONTAINER, PluginDescriptions.SphraivaPlugin.NAME);
        await context.EmitEventAsync(OutputEvents.SETUP_INFRASTRUCTURE_FAILED, data: "Setup not possible.");
        return null;
    }

    private static async Task<string> GetRepositoryPathAsync(Kernel kernel, string cloneRepositoryResult)
    {
        var chatHistory = new ChatHistory(SYSTEM_PROMPT);

        chatHistory.AddUserMessage(cloneRepositoryResult);

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var repositoryPath = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
        return repositoryPath.Content!;
    }
}