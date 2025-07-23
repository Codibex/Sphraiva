using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace MCP.Host.Agents.Steps;

public class ImplementationStep : KernelProcessStep
{
    private const string SYSTEM_PROMPT =
        """
        You are a coding agent with full access to tools and a development container environment. 
        You are tasked with implementing planned changes in the cloned repository within the development container in the workspace directory.
        
        A Git repository exists in a subfolder under `/workspace` in the development container.
        Use the tools to make the required changes.
        
        The tools available to you include:
        - Run command in dev container: Can be used for searching and updating files
        - Git command in dev container: Can be used for git command execution.
        
        **Begin by identifying the repository path using the appropriate tool.  
        Use actual tool invocations from this point forward.  
        Do not simulate or suggest commands — execute them.**
        
        Use the provided change plan below to guide your implementation:
        ---
        {PlannedChanges}
        ---

        Instructions:
        - For each change, provide the actual code modifications as code blocks, including the file name and the location within the file.
        - If any file or method listed in the change plan cannot be found, report it in a section titled "Unresolved Changes" at the end of your response.
        - Only output the code changes, not a step-by-step description.
        - Create a development branch with a short name reflecting the required changes.
        - Commit each change with a meaningful message.
        - Build the solution and fix any build issues.
        - Push the changes to the remote repository only after all planned changes are implemented, the solution builds successfully, and all tests pass.
        - Respond in Markdown format, using code blocks for code changes and clear file references.
        """;

    public static class OutputEvents
    {
        public const string CHANGE_ANALYSIS_FINISHED = nameof(CHANGE_ANALYSIS_FINISHED);
    }

    [KernelFunction]
    public async Task ImplementChangesAsync(Kernel kernel, KernelProcessStepContext context, CodingProcessContext codingProcessContext)
    {
        var logger = kernel.GetRequiredService<ILogger<InputCheckStep>>();
        logger.LogInformation("Implement planned changes");

        // Insert planned changes directly into the prompt for clarity
        var prompt = SYSTEM_PROMPT.Replace("{PlannedChanges}", codingProcessContext.PlannedChanges ?? "<no planned changes provided>");
        var chatHistory = new ChatHistory(prompt);
        chatHistory.AddSystemMessage($"Container: {codingProcessContext.ContainerName}");
        chatHistory.AddUserMessage($"Repository: {codingProcessContext.RepositoryName}");

        var settings = new OllamaPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var response = await chatCompletionService.GetChatMessageContentAsync(chatHistory, settings, kernel);

        if (response is null)
        {
            throw new InvalidOperationException("Chat completion response is null.");
        }

        await context.EmitEventAsync(OutputEvents.CHANGE_ANALYSIS_FINISHED, data: codingProcessContext);
    }
}
