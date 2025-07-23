using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MCP.Host.Agents.Steps;

public class ImplementationStep : KernelProcessStep
{
    private const string SYSTEM_PROMPT =
        """
        Implement the planned changes in the cloned repository within the development container in the workspace directory.
        - Use the provided change plan to guide your implementation.
        - Create a development branch with a short name reflecting the required changes.
        - For each change, update the relevant files and commit with a meaningful message.
        - Build the solution and fix any build issues.
        - Push the changes to the remote repository only after all planned changes are implemented, the solution builds successfully, and all tests pass.
        """;
    //"""
    //Your job is to implement the planned changes in the cloned repository within the development container.
    //- Use the provided change plan to guide your implementation.
    //- Create a development branch with a short name reflecting the required changes.
    //- For each change, update the relevant files and commit with a meaningful message.
    //- Build the solution and fix any build issues.
    //-Push the changes to the remote repository only after all planned changes are implemented, the solution builds successfully, and all tests pass.
    //If information is missing or unclear, ask clarifying questions.
    //Respond with a summary of the actions taken and any issues encountered.
    //""";

    public static class OutputEvents
    {
        public const string CHANGE_ANALYSIS_FINISHED = nameof(CHANGE_ANALYSIS_FINISHED);
    }

    [KernelFunction]
    public async Task ImplementChangesAsync(Kernel kernel, KernelProcessStepContext context, CodingProcessContext codingProcessContext)
    {
        var logger = kernel.GetRequiredService<ILogger<InputCheckStep>>();
        logger.LogInformation("Implement planned changes");

        var chatHistory = new ChatHistory(SYSTEM_PROMPT);
        chatHistory.AddSystemMessage($"Container: {codingProcessContext.ContainerName}");
        chatHistory.AddUserMessage($"Repository: {codingProcessContext.RepositoryName}");
        chatHistory.AddUserMessage($"Requirement: {codingProcessContext.Requirement}");
        chatHistory.AddSystemMessage($"Planned changes: {codingProcessContext.PlannedChanges}");

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var response = await chatCompletionService.GetChatMessageContentAsync(chatHistory);

        if (response is null)
        {
            throw new InvalidOperationException("Chat completion response is null.");
        }

        await context.EmitEventAsync(OutputEvents.CHANGE_ANALYSIS_FINISHED, data: codingProcessContext);
    }
}
