using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MCP.Host.Agents.Steps;

public class InputCheckStep : KernelProcessStep<InputCheckState>
{
    private InputCheckState _state = new();

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

    public static class ProcessFunctions
    {
        public const string CHECK_INPUT = nameof(CHECK_INPUT);
    }

    public static class OutputEvents
    {
        public const string INPUT_VALIDATION_SUCCEEDED = nameof(INPUT_VALIDATION_SUCCEEDED);
        public const string INPUT_VALIDATION_FAILED = nameof(INPUT_VALIDATION_FAILED);
    }

    public override ValueTask ActivateAsync(KernelProcessStepState<InputCheckState> state)
    {
        _state = state.State!;
        _state.ChatHistory ??= new ChatHistory(SYSTEM_PROMPT);

        return base.ActivateAsync(state);
    }

    [KernelFunction(ProcessFunctions.CHECK_INPUT)]
    public async Task CheckInputAsync(Kernel kernel, KernelProcessStepContext context, string requirement)
    {
        var logger = kernel.GetRequiredService<ILogger<InputCheckStep>>();
        logger.LogInformation("Verify requirement");

        _state.ChatHistory!.AddUserMessage(requirement);

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var response = await chatCompletionService.GetChatMessageContentAsync(_state.ChatHistory!);
        
        if (response is null)
        {
            throw new InvalidOperationException("Chat completion response is null.");
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
            throw;
        }

        if (checkResult is null)
        {
            throw new InvalidOperationException("Deserialized InputCheckResult object is null.");
        }

        if (checkResult.MissingParameters.Count == 0)
        {
            await context.EmitEventAsync(OutputEvents.INPUT_VALIDATION_SUCCEEDED, data: checkResult, KernelProcessEventVisibility.Public);
            return;
        }

        await context.EmitEventAsync(OutputEvents.INPUT_VALIDATION_FAILED, data: checkResult.MissingParameters, KernelProcessEventVisibility.Public);
    }
}

public class InputCheckResult
{
    public required string InstructionName { get; set; }
    public required string RepositoryName { get; set; }
    public required string Requirement { get; set; }
    public required ICollection<string> MissingParameters { get; set; } = [];
}

public class InputCheckState
{
    public ChatHistory? ChatHistory { get; set; }
}
