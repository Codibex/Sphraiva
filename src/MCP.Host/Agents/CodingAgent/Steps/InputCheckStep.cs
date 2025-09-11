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
        ---
        **TASK: STRICT INPUT VALIDATION**
        ---

        ## **RULES (MANDATORY)**

        1. **NO MODIFICATIONS**:
           - Output values **exactly as provided** (including typos, case, and formatting).

        2. **MATCHING**:
           - Extract values from the entire text.
           - Check if parts of the text can be matched with the output keys.
           - Check if the whole text or parts of the text can be the requirement.

        3. **OUTPUT FORMAT**:
           - Respond **only** in valid JSON:
             ```json
             {
               "InstructionName": "<exact_value>",
               "RepositoryName": "<exact_value>",
               "Requirement": "<exact_value>",
               "MissingParameters": ["<missing_key_1>", "<missing_key_2>"]  // Empty if none
             }
             ```
           - Set values to null if not found.
           - All keys from the output are required, so if any of them cannot be found (e.g. `null`) or matched, it is missing.
           - **No additional text or explanations**.

        4. **MISSING PARAMETERS**:
           - List missing keys in `MissingParameters`.
           - If all keys are present, set `MissingParameters: []`.

        ---
        
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
            const string ERROR_MESSAGE = "Response from agent is not valid.";
            logger.LogError(ERROR_MESSAGE);
            await context.EmitEventAsync(OutputEvents.INPUT_VALIDATION_FAILED, data: ERROR_MESSAGE);
            return;
        }

        InputCheckResult? checkResult;
        try
        {
            var regex = new Regex(@"(<\/think>)*\s*^\s*({[\s\S]*?})\s*$", RegexOptions.Multiline);
            var match = regex.Match(response.Content!.Trim());
            var json = match.Success ? match.Groups[match.Groups.Count - 1].Value.Trim() : response.Content!.Trim();

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
