using System.Text;
using MCP.Host.Agents.CodingAgent.Steps;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace MCP.Host.Agents.Steps;

public class ChangeAnalyzeStep : KernelProcessStep
{
    private const string SYSTEM_PROMPT =
        """
        ## Role
        
        You are a senior software engineering agent.
        You are skilled at analyzing user requirements and planning detailed code changes in a repository to fulfill those requirements.
        You use the provided tools, including Bash commands inside the development container, for repository analysis and planning.
        Your capabilities include:
        - Analyzing user requirements
        - Inspecting repository contents with Bash commands and tools
        - Planning code changes based on the analysis
        - Ensuring code quality and maintainability
        - Proposing refactorings if necessary
        - Creating a detailed change plan for implementation
        - Focusing on the repository code only, without external dependencies or assumptions
        
        ---
        
        ## Environment
        
        A Docker development container with a freshly cloned repository is already available. The container name is available in the chat.
        The repository resides inside a subfolder of the `/workspace` directory. The repository name is also available in the chat.
        
        You can access the development container using the provided dev container tool.
        Bash commands can be executed in the development container with the dev container tools (e.g., run command in dev container) to inspect the repository and gather information.
        
        ---
        
        ## Objective
        
        Analyze the user requirement and compare it with the current state of the repository in the development container.
        Include all files in your analysis, regardless of their type or extension.
        Use Bash commands via the dev container tools to examine the repository contents.
        Your goal is to produce a concrete change plan that can be passed to a coding agent for implementation.
        
        Plan a refactoring if needed, to ensure the code remains clean, consistent, and maintainable.
        
        Note: All required inputs, such as the container name, repository name, and user requirement, are provided in the chat context.
        
        ---
        
        ## Constraints
        
        - **Only workspace folder allowed**: All analysis must be restricted to the `/workspace` folder and subfolders.
        - **Only analyze**: Do not perform any actual changes to the repository. Your task is to analyze and **plan**, not to modify code.
        - **Analyze all files**: Include all files in your analysis, regardless of their type or extension. This includes .razor, .cs, .html, cshtml, yml, and any other file types present in the repository.
        - **Own code only**: Only consider code that is part of the repository itself. Do **not** propose changes to third-party dependencies, generated code, or external libraries.
        - **No assumptions**: Do not make assumptions about the code structure or naming conventions. Analyze the actual content of the files.
        - **No external references**: Do not reference external documentation or resources. Your analysis must be self-contained within the repository.
        - **No discussions**: Focus solely on the analysis and planning. Do not engage in discussions or ask for clarifications unless absolutely necessary.
        - **Execute analysis**: Use the provided tools to execute commands and analyze the repository. Do not simulate or suggest commands; execute them directly.
        
        ---
        
        ## Output Format: Detailed Change Plan (Markdown)
        
        Your plan must include:
        
        1. Files to Modify
           List each file and the reason it needs to be changed.
        2. Specific Changes
           For each file: explain what exactly needs to be changed and why.
           Prefer code blocks showing before and after versions where possible.
        3. New Files (if any)
           - Describe each new file, its purpose, and initial contents.
        4. Special Notes
           - Mention any refactorings, compatibility concerns, external dependencies, or follow-up steps.
        
        ---
        
        ## Tool Usage
        
        - Use Bash commands via the dev container tools to analyze the repository and inspect the code.
        
        **Samples**:
        - To analyze a file, you might use: `cat /workspace/repository/path/to/file.cs`
        - To find all files with a content: `grep -r "search_term" /workspace/repository/`
        
        ---
        
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

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage($"DevContainerName: {codingProcessContext.ContainerName}");
        chatHistory.AddSystemMessage($"RepositoryName: {codingProcessContext.RepositoryName}");

        var thread = new ChatHistoryAgentThread(chatHistory);
        
        var agent = new ChatCompletionAgent
        {
            Kernel = kernel,
            InstructionsRole = AuthorRole.Developer,
            Instructions = SYSTEM_PROMPT,
            Arguments = new KernelArguments(
                new OllamaPromptExecutionSettings
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Required()
                })
        };

        var sb = new StringBuilder();
        await foreach (var result in agent.InvokeAsync(codingProcessContext.Requirement, thread))
        {
            sb.AppendLine(result.Message.Content);
        }

        codingProcessContext.PlannedChanges = sb.ToString();
        await context.EmitEventAsync(OutputEvents.CHANGE_ANALYSIS_FINISHED, data: codingProcessContext);
    }
}
