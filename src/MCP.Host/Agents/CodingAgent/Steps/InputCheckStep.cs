using MCP.Host.Agents.Steps;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MCP.Host.Agents.CodingAgent.Steps;

public class InputCheckStep : KernelProcessStep
{
    private const string SYSTEM_PROMPT =
        """
        Your job is to check if all necessary parameters provided by the user.
        The following parameters are required:
        - Instruction name for the docker container creation.
        - A repository name.
        - A set of requirements for changes.
        Respond with a json including the following properties:
        - InstructionName: string, the instruction name for the docker container creation.
        - RepositoryName: string, the repository name.
        - Requirement: string, the set of requirements for changes.
        - MissingParameters: string[], an array of missing parameters.
        """;

    public static class ProcessStepFunctions
    {
        public const string CHECK_INPUT = nameof(CHECK_INPUT);
    }

    public static class OutputEvents
    {
        public const string INPUT_VALIDATION_SUCCEEDED = nameof(INPUT_VALIDATION_SUCCEEDED);
        public const string INPUT_VALIDATION_FAILED = nameof(INPUT_VALIDATION_FAILED);
    }

    [KernelFunction(ProcessStepFunctions.CHECK_INPUT)]
    public async Task CheckInputAsync(Kernel kernel, KernelProcessStepContext context, string requirement)
    {
        var logger = kernel.GetRequiredService<ILogger<InputCheckStep>>();
        logger.LogInformation("Verify requirement");

        var chatHistory = new ChatHistory(SYSTEM_PROMPT);

        chatHistory.AddUserMessage(requirement);

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var response = await chatCompletionService.GetChatMessageContentAsync(chatHistory);

        if (string.IsNullOrWhiteSpace(response.Content))
        {
            logger.LogError("Response from agent is not valid.");
            await context.EmitEventAsync(OutputEvents.INPUT_VALIDATION_FAILED, data: "Response from agent is not valid.");
            return;
        }

        InputCheckResult? checkResult;
        try
        {
            var regex = new Regex(@"^```(?:json)?\s*([\s\S]*?)\s*```$", RegexOptions.Multiline);
            var match = regex.Match(response.Content!);
            var json = match.Success ? match.Groups[1].Value.Trim() : response.Content!.Trim();

            checkResult = JsonSerializer.Deserialize<InputCheckResult>(json);
        }
        catch(Exception e)
        {
            logger.LogError(e, "Failed to deserialize InputCheckResult from response: {ResponseContent}", response.Content);
            await context.EmitEventAsync(OutputEvents.INPUT_VALIDATION_FAILED, data: e.Message);
            return;
        }

        if (checkResult is null)
        {
            logger.LogError("Deserialized InputCheckResult object is null.");
            await context.EmitEventAsync(OutputEvents.INPUT_VALIDATION_FAILED, data: "Response from agent is not valid.");
            return;
        }

        if (checkResult.MissingParameters.Count == 0)
        {
            await context.EmitEventAsync(OutputEvents.INPUT_VALIDATION_SUCCEEDED, data: checkResult);
            return;
        }

        logger.LogError("Input has missing parameters: {MissingParameters}", checkResult.MissingParameters);
        await context.EmitEventAsync(OutputEvents.INPUT_VALIDATION_FAILED, data: checkResult.MissingParameters);
    }
}
