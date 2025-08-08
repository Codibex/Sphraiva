using MCP.Host.Agents.Steps;
using MCP.Host.Hubs;
using MCP.Host.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.Connectors.Ollama;
using System.Reflection.Metadata;

namespace MCP.Host.Agents;

public class CodingFlowProcess(IKernelFactory kernelFactory, IHubContext<CodingAgentHub, ICodingAgentHub> hubContext)
{
    public async Task RunAsync(FlowParameter parameter)
    {


        // Plugin parameter can be false and added for specific agents
        var kernel = kernelFactory.Create(true);
        
        const string MANAGER_AGENT_NAME = "MANAGER_AGENT";
        var managerAgent = CreateAgent(MANAGER_AGENT_NAME, MANAGER_AGENT_INSTRUCTIONS, kernel.Clone());

        const string ANALYSIS_AGENT_NAME = "ANALYSIS_AGENT";
        var analysisAgent = CreateAgent(ANALYSIS_AGENT_NAME, ANALYSIS_AGENT_INSTRUCTIONS, kernel.Clone());

        const string IMPLEMENTATION_AGENT_NAME = "IMPLEMENTATION_AGENT";
        var implementationAgent = CreateAgent(IMPLEMENTATION_AGENT_NAME, IMPLEMENTATION_AGENT_INSTRUCTIONS, kernel.Clone());

        var selectionFunction = AgentGroupChat.CreatePromptFunctionForStrategy(
            $$$"""
             Determine which participant takes the next turn in a conversation based on the the most recent participant.
             State only the name of the participant to take the next turn.
             No participant should take more than one turn in a row.
             
             Choose only from these participants:
             - {{{ANALYSIS_AGENT_NAME}}}
             - {{{IMPLEMENTATION_AGENT_NAME}}}
             
             Always follow these rules when selecting the next participant:
             - After user input, it is {{{ANALYSIS_AGENT_NAME}}}'s turn.
             - After {{{ANALYSIS_AGENT_NAME}}}, it is {{{IMPLEMENTATION_AGENT_NAME}}}'s turn.
             - After {{{IMPLEMENTATION_AGENT_NAME}}}, it is {{{ANALYSIS_AGENT_NAME}}}'s turn.

             History:
             {{$history}}
             """);

        var terminationFunction = AgentGroupChat.CreatePromptFunctionForStrategy(
            $$$"""
            Evaluate if the {{{IMPLEMENTATION_AGENT_NAME}}} has confirmed that all required changes have been successfully completed.
            Look for a final confirmation from the {{{IMPLEMENTATION_AGENT_NAME}}}, such as "implementation complete", "all changes applied", or a similar statement in the conversation history.
            If such a confirmation is present, respond with the phrase: completed flow

            History:
            {{$history}}
            """);

        var chat = new AgentGroupChat(analysisAgent, implementationAgent)
        {
            ExecutionSettings = new AgentGroupChatSettings
            {
                SelectionStrategy = new KernelFunctionSelectionStrategy(selectionFunction, kernel)
                {
                    HistoryVariableName = "history",
                    ResultParser = (r) => r.GetValue<string>() ?? string.Empty
                },
                TerminationStrategy = new KernelFunctionTerminationStrategy(terminationFunction, kernel)
                {
                    HistoryVariableName = "history",
                    ResultParser = (r) =>
                        r.GetValue<string>()?.Contains("completed flow", StringComparison.InvariantCultureIgnoreCase) ??
                        false
                }
            }
        };

        var kernel2 = kernelFactory.CreateAgentGroupChatKernel(managerAgent, chat);
        
        KernelProcess process = SetupProcess(parameter.ChatId);
        IExternalKernelProcessMessageChannel processMessageChannel = new CodingAgentProcessMessageChannel(parameter.ConnectionId, hubContext);

        
        await process.StartAsync(kernel2,
            new KernelProcessEvent
            {
                Id = GatherRequirementStep.START_REQUIREMENT_IMPLEMENTATION,
                Data = parameter.Requirement
            },
            processMessageChannel);
    }

    private KernelProcess SetupProcess(Guid chatId)
    {
        ProcessBuilder processBuilder = new($"CodingAgent-{chatId}");

        return processBuilder.Build();
    }

    private static ChatCompletionAgent CreateAgent(string agentName, string instructions, Kernel kernel) =>
        new()
        {
            Name = agentName,
            Instructions = instructions,
            Kernel = kernel,
            Arguments = new KernelArguments(
                new OllamaPromptExecutionSettings
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                    Temperature = 0
                })
        };

    private const string MANAGER_AGENT_INSTRUCTIONS =
        """
        Capture information provided by the user for their requirement request.
        Request confirmation without suggesting additional details.
        Once confirmed inform them you're working on the request.
        Never provide a direct answer to the user's request.
        """;

    private const string ANALYSIS_AGENT_INSTRUCTIONS =
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

    private const string IMPLEMENTATION_AGENT_INSTRUCTIONS =
        """
        ## Role
        
        You are a senior software engineering agent.
        You are skilled at implementing planned changes in a cloned repository within a development container. 
        You use the provided tools, including Bash commands inside the development container, for code modifications and repository management.
        Your capabilities include:
        - Analyzing planned changes
        - Implementing code changes based on a detailed change plan
        - Ensuring code quality and maintainability
        - Commit changes with a concise message summarizing the purpose
        - Building the solution to verify changes
        - Running tests to ensure correctness
        - Pushing changes to the remote repository after successful implementation and testing
        
        ---
        
        ## Environment
        
        A Docker development container with a freshly cloned repository is already available. The container name is available in the chat.
        The repository resides inside a subfolder of the `/workspace` directory. The repository name is also available in the chat.
        
        You can access the development container using the provided dev container tool.
        Bash commands can be executed in the development container with the dev container tools (e.g., run command in dev container) to inspect the repository, gather information make changes and manage the repository.
        
        ---
        
        ## Objective
        
        Create a new branch named using the pattern feature/<short-description> (e.g., feature/rename-send-method).
        Analyze the planned changes provided in the chat and implement them in the repository within the development container.
        Use Bash commands via the dev container tools to modify files, commit changes, and manage the repository.
        Ensure that all changes are made according to the provided change plan, maintaining code quality and consistency.
        
        Note: All required inputs, such as the container name, repository name, and planned changes, are provided in the chat context.
        
        ---
        
        ## Constraints
        
        - **Only workspace folder allowed**: All changes must be restricted to the `/workspace` folder and subfolders.
        - **Only implement changes**: Do not perform any analysis or planning. Your task is to implement the planned changes in the repository.
        - **Own code only**: Only consider code that is part of the repository itself. Do **not** modify third-party dependencies, generated code, or external libraries unless explicitly included in the planned changes.
        - **No assumptions**: Do not make assumptions about the code structure or naming conventions. Follow the provided change plan and repository structure.
        - **No external references**: Do not reference external documentation or resources. Your implementation must be self-contained within the repository.
        - **No discussions**: Focus solely on the implementation of the planned changes. Do not engage in discussions or ask for clarifications unless absolutely necessary.
        - **Commit changes**: After implementing the changes, commit them with meaningful commit messages that reflect the changes made.
        - **Build and test**: Build the solution to verify that the changes are correct. Fix the issue while staying within the scope of the planned changes.
        - **Push changes**: After successful implementation and testing, push the changes to the remote repository.
        
        ---
        
        ## Tool Usage
        
        - Use the dev container tools to run Bash commands for modifying files, committing changes, and managing the repository.
        
        ---
                
        """;
}

public record FlowParameter(Guid ChatId, string ConnectionId, string Requirement);
