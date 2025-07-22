using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MCP.Host.Agents.Steps;

public class ChangeAnalyzeStep : KernelProcessStep
{
    private const string SYSTEM_PROMPT =
        """
        Your job is to analyze the provided requirements and compare them with the current state of the cloned repository in the development container.
        Identify and list the specific changes that need to be made in the repository to fulfill the requirements.
        For each required change, specify:
        - Which files or components need to be modified, added, or removed.
        - What exactly needs to be changed (e.g., add a method, update configuration, fix a bug).
        Respond in Markdown as a numbered list.
        """;
    //"""
    //Your job is to analyze the provided requirements and compare them with the current state of the cloned repository in the development container.
    //    Identify and list the specific changes that need to be made in the repository to fulfill the requirements.
    //    For each required change, specify:
    //    - Which files or components need to be modified, added, or removed.
    //    - What exactly needs to be changed (e.g., add a method, update configuration, fix a bug).
    //    If information is missing or unclear, ask clarifying questions.
    //    Respond in Markdown as a numbered list.
    //""";

    public static class OutputEvents
    {
        public const string CHANGE_ANALYSIS_FINISHED = nameof(CHANGE_ANALYSIS_FINISHED);
    }

    [KernelFunction]
    public async Task AnalyzeChangesAsync(Kernel kernel, KernelProcessStepContext context, CodingProcessContext codingProcessContext)
    {
        var logger = kernel.GetRequiredService<ILogger<InputCheckStep>>();
        logger.LogInformation("Analyze changes");

        var chatHistory = new ChatHistory(SYSTEM_PROMPT);
        chatHistory.AddSystemMessage($"Container: {codingProcessContext.ContainerName}");
        chatHistory.AddUserMessage($"Repository: {codingProcessContext.RepositoryName}");
        chatHistory.AddUserMessage($"Requirement: {codingProcessContext.Requirement}");

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var response = await chatCompletionService.GetChatMessageContentAsync(chatHistory);

        if (response is null)
        {
            throw new InvalidOperationException("Chat completion response is null.");
        }

        codingProcessContext.PlannedChanges = response.Content;

        await context.EmitEventAsync(OutputEvents.CHANGE_ANALYSIS_FINISHED, data: codingProcessContext);
    }
}
