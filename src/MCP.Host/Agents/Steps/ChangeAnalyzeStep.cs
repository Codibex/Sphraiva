using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace MCP.Host.Agents.Steps;

public class ChangeAnalyzeStep : KernelProcessStep
{
    private const string SYSTEM_PROMPT =
        """
        You are an expert coding agent tasked with analyzing a cloned Git repository to determine the exact changes needed to implement a specific requirement.

        A Git repository exists in a subfolder under `/workspace` in the development container.  
        Use the development container run command tool to locate the repository directory and inspect its code.

        **Focus only on your own codebase** and **exclude any framework code** or code from libraries like `HttpClient`.
        Your task is to analyze and modify only the parts of the code you have implemented (custom methods, classes, etc.).
        
        Use the tools to:
        1. Search the repository recursively for methods or classes that are relevant to the provided requirement.
        2. Identify and list all files that contain methods like `Send` (or similar).
        3. Inspect the code and compare it with the given requirements.

        Do not assume the repository location — actively locate it using the tools available.

        ---

        ## Your Goal:
        Analyze the repository at `/workspace` and determine the **exact and complete list of source code changes** required to implement the following requirement:

        ---
        {Requirement}
        ---

        ## Instructions:
        1. **Actively use the tools** provided to:
           - Recursively search the repository for methods like `Send` or similar that need modification.
           - **Only consider your own codebase** — do not modify or analyze any framework methods like those from `HttpClient`.
           - Also inspect .razor and .cshtml files. These may contain relevant C# logic inside @code { ... } blocks.
           - Read relevant files and understand the current implementation.
           - Locate all method declarations, calls, interface implementations, or overrides.
           - Ensure that the identified files and methods are correct by dynamically verifying the file paths and method locations.
        2. **Do NOT assume anything.** Use the tools to verify all occurrences of code needing modification.
           Do **not** include files that just contain search matches without any necessary modification.
        3. **Do NOT make the changes yet.** Only output a precise plan.

        ---

        ## Output Format:
        Output your findings in **Markdown**, using the following format:

        ```markdown
        ## Summary of Current Implementation
        [Short summary of where and how the relevant code is implemented.]

        ---

        ## Required Code Changes

        - [ ] **File**: `relative/path/to/File.cs`
          - **Change**: [E.g. Rename `Send` method to `SendAsync`]
          - **Location**: Line 42
          - **Before**:
            ```csharp
            public void Send(string message)
            ```
          - **After**:
            ```csharp
            public async Task SendAsync(string message)
            ```
          - **Notes**: [Optional — interface impact, reflection use, dynamic dispatch, etc.]

        - [ ] **File**: ...
        ```
        """;

    public static class OutputEvents
    {
        public const string CHANGE_ANALYSIS_FINISHED = nameof(CHANGE_ANALYSIS_FINISHED);
    }

    [KernelFunction]
    public async Task AnalyzeChangesAsync(Kernel kernel, KernelProcessStepContext context, CodingProcessContext codingProcessContext)
    {
        var logger = kernel.GetRequiredService<ILogger<InputCheckStep>>();
        logger.LogInformation("Analyze changes");

        var prompt = SYSTEM_PROMPT.Replace("{Requirement}", codingProcessContext.Requirement);
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

        codingProcessContext.PlannedChanges = response.Content;

        await context.EmitEventAsync(OutputEvents.CHANGE_ANALYSIS_FINISHED, data: codingProcessContext);
    }
}
