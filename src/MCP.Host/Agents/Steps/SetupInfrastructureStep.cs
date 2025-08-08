using System.Text.RegularExpressions;
using MCP.Host.Plugins;
using Microsoft.SemanticKernel;

namespace MCP.Host.Agents.Steps;

public class SetupInfrastructureStep : KernelProcessStep
{
    public static class OutputEvents
    {
        public const string SETUP_INFRASTRUCTURE_SUCCEEDED = nameof(SETUP_INFRASTRUCTURE_SUCCEEDED);
    }

    [KernelFunction]
    public async Task SetupInfrastructureAsync(Kernel kernel, KernelProcessStepContext context, InputCheckResult input)
    {
        var logger = kernel.GetRequiredService<ILogger<InputCheckStep>>();
        logger.LogInformation("Setup infrastructure");

        if (!kernel.Plugins.TryGetPlugin(PluginDescriptions.SphraivaPlugin.NAME, out var plugin))
        {
            throw new InvalidOperationException($"{PluginDescriptions.SphraivaPlugin.NAME} plugin is not available.");
        }

        var result = await CreateContainerAsync(plugin, input);

        var match = Regex.Match(result!.ToString()!, @"Started container successfully: (\S+)");
        if (!match.Success)
        {
            throw new InvalidOperationException("Failed to parse container creation result.");
        }

        var codingProcessContext = new CodingProcessContext
        {
            ContainerName = match.Groups[1].Value.Trim('"'),
            RepositoryName = input.RepositoryName,
            Requirement = input.Requirement
        };

        await CloneRepositoryAsync(plugin, codingProcessContext);


        await context.EmitEventAsync(OutputEvents.SETUP_INFRASTRUCTURE_SUCCEEDED, data: new
        {
            codingProcessContext.RepositoryName,
            codingProcessContext.ContainerName,
            codingProcessContext.Requirement
        });
    }

    private static async Task<object?> CreateContainerAsync(KernelPlugin plugin, InputCheckResult input)
    {
        if (!plugin.TryGetFunction(PluginDescriptions.SphraivaPlugin.Functions.CREATE_DEV_CONTAINER, out var function))
        {
            throw new InvalidOperationException($"Function {PluginDescriptions.SphraivaPlugin.Functions.CREATE_DEV_CONTAINER} is not available in {PluginDescriptions.SphraivaPlugin.NAME} plugin.");
        }

        var arguments = new KernelArguments
        {
            ["instructionName"] = input.InstructionName,
        };

        return await function.InvokeAsync(arguments);
    }

    private static async Task CloneRepositoryAsync(KernelPlugin plugin, CodingProcessContext codingProcessContext)
    {
        if (!plugin.TryGetFunction(PluginDescriptions.SphraivaPlugin.Functions.CLONE_REPOSITORY_IN_DEV_CONTAINER, out var function))
        {
            throw new InvalidOperationException($"Function {PluginDescriptions.SphraivaPlugin.Functions.CREATE_DEV_CONTAINER} is not available in {PluginDescriptions.SphraivaPlugin.NAME} plugin.");
        }

        var arguments = new KernelArguments
        {
            ["containerName"] = codingProcessContext.ContainerName,
            ["repositoryName"] = codingProcessContext.RepositoryName,
        };

        await function.InvokeAsync(arguments);
    }
}