using MCP.Host.Agents.Steps;
using MCP.Host.Hubs;
using MCP.Host.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using System.ComponentModel;
using System.Text.Json;

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

        var gatherRequirementStep = processBuilder.AddStepFromType<GatherRequirementStep>();
        var inputCheckStep = processBuilder.AddStepFromType<InputCheckStep>();
        var setupInfrastructureStep = processBuilder.AddStepFromType<SetupInfrastructureStep>();

        var managerAgentStep = processBuilder.AddStepFromType<ManagerAgentStep>();
        var agentGroupStep = processBuilder.AddStepFromType<AgentGroupChatStep>();

        var proxyStep = processBuilder.AddProxyStep("workflowProxy", [CodingAgentProcessTopics.REQUEST_REQUIREMENT_UPDATE, "RequestUserReview", "PublishDocumentation"]);

        processBuilder
            .OnInputEvent(GatherRequirementStep.START_REQUIREMENT_IMPLEMENTATION)
            .SendEventTo(new(gatherRequirementStep));

        // Hooking up the process steps
        gatherRequirementStep
            .OnFunctionResult()
            .SendEventTo(new ProcessFunctionTargetBuilder(inputCheckStep, functionName: InputCheckStep.ProcessFunctions.CHECK_INPUT));

        inputCheckStep
            .OnEvent(InputCheckStep.OutputEvents.INPUT_VALIDATION_FAILED)
            .EmitExternalEvent(proxyStep, CodingAgentProcessTopics.REQUEST_REQUIREMENT_UPDATE);

        inputCheckStep
            .OnEvent(InputCheckStep.OutputEvents.INPUT_VALIDATION_SUCCEEDED)
            .SendEventTo(new ProcessFunctionTargetBuilder(setupInfrastructureStep));

        setupInfrastructureStep
            .OnEvent(SetupInfrastructureStep.OutputEvents.SETUP_INFRASTRUCTURE_SUCCEEDED)
            .SendEventTo(new ProcessFunctionTargetBuilder(managerAgentStep,
                ManagerAgentStep.ProcessStepFunctions.InvokeAgent));

        // Delegate to inner agents
        managerAgentStep
            .OnEvent(AgentOrchestrationEvents.AgentWorking)
            .SendEventTo(new ProcessFunctionTargetBuilder(managerAgentStep, ManagerAgentStep.ProcessStepFunctions.InvokeGroup));

        // Provide input to inner agents
        managerAgentStep
            .OnEvent(AgentOrchestrationEvents.GroupInput)
            .SendEventTo(new ProcessFunctionTargetBuilder(agentGroupStep, parameterName: "input"));

        // Provide inner response to primary agent
        agentGroupStep
            .OnEvent(AgentOrchestrationEvents.GroupCompleted)
            .SendEventTo(new ProcessFunctionTargetBuilder(managerAgentStep, ManagerAgentStep.ProcessStepFunctions.ReceiveResponse, parameterName: "response"));

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
        ## Role
        
        You are the manager agent responsible for orchestrating the coding process.
        
        ---
        
        ## Environment
        
        It is not possible to ask the user anything.
        
        ---
        
        ## Objective
        
        - Coordinate the flow between analysis and implementation agents.
        - Pass requirements and results between agents.
        - Monitor progress and ensure all steps are completed.
        - Do not ask the user for confirmation or input.
        - Do not provide direct answers to the user's requirement.
        - Do not suggest additional details.
        - Only orchestrate the process and communicate between agents.
        
        ---
        
        ### Constraints
        
        - Never answer user requirements directly.
        - Never provide code suggestions or solutions.
        - Only delegate tasks to other agents and monitor their progress.
        
        ---
        
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
        
        It is not possible to ask the user anything.
        
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
        
        It is not possible to ask the user anything.
        
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

/// <summary>
/// Primary agent. This agent is responsible for managing the flow of the coding process.
/// </summary>
public class ManagerAgentStep : KernelProcessStep
{
    public const string AgentServiceKey = $"{nameof(ManagerAgentStep)}:{nameof(AgentServiceKey)}";
    public const string ReducerServiceKey = $"{nameof(ManagerAgentStep)}:{nameof(ReducerServiceKey)}";

    public static class ProcessStepFunctions
    {
        public const string InvokeAgent = nameof(InvokeAgent);
        public const string InvokeGroup = nameof(InvokeGroup);
        public const string ReceiveResponse = nameof(ReceiveResponse);
    }

    [KernelFunction(ProcessStepFunctions.InvokeAgent)]
    public async Task InvokeAgentAsync(KernelProcessStepContext context, Kernel kernel, string userInput, ILogger logger)
    {
        // Get the chat history
        IChatHistoryProvider historyProvider = GetHistory(kernel);
        ChatHistory history = historyProvider.Get();
        ChatHistoryAgentThread agentThread = new(history);

        // Obtain the agent response
        ChatCompletionAgent agent = GetAgent<ChatCompletionAgent>(kernel, AgentServiceKey);
        await foreach (ChatMessageContent message in agent.InvokeAsync(new ChatMessageContent(AuthorRole.User, userInput), agentThread))
        {
            // Both the input message and response message will automatically be added to the thread, which will update the internal chat history.

            // Emit event for each agent response
            await context.EmitEventAsync(new() { Id = AgentOrchestrationEvents.AgentResponse, Data = message });
        }

        // Evaluate current intent
        IntentResult intent = await IsRequestingUserInputAsync(kernel, history, logger);

        string intentEventId =
            intent.IsRequestingUserInput ?
                AgentOrchestrationEvents.AgentResponded :
                intent.IsWorking ?
                    AgentOrchestrationEvents.AgentWorking :
                    CommonEvents.UserInputComplete;

        await context.EmitEventAsync(new() { Id = intentEventId });
    }

    [KernelFunction(ProcessStepFunctions.InvokeGroup)]
    public async Task InvokeGroupAsync(KernelProcessStepContext context, Kernel kernel)
    {
        // Get the chat history
        IChatHistoryProvider historyProvider = GetHistory(kernel);
        ChatHistory history = historyProvider.Get();

        // Summarize the conversation with the user to use as input to the agent group
        string summary = await SummarizeHistoryAsync(kernel, ReducerServiceKey, history);

        await context.EmitEventAsync(new() { Id = AgentOrchestrationEvents.GroupInput, Data = string.Join(Environment.NewLine, history) });
    }

    [KernelFunction(ProcessStepFunctions.ReceiveResponse)]
    public async Task ReceiveResponseAsync(KernelProcessStepContext context, Kernel kernel, string response)
    {
        // Get the chat history
        IChatHistoryProvider historyProvider = GetHistory(kernel);
        ChatHistory history = historyProvider.Get();

        // Proxy the inner response
        ChatCompletionAgent agent = GetAgent<ChatCompletionAgent>(kernel, AgentServiceKey);
        ChatMessageContent message = new(AuthorRole.Assistant, response) { AuthorName = agent.Name };
        history.Add(message);

        await context.EmitEventAsync(new() { Id = AgentOrchestrationEvents.AgentResponse, Data = message });

        await context.EmitEventAsync(new() { Id = AgentOrchestrationEvents.AgentResponded });
    }

    private static async Task<IntentResult> IsRequestingUserInputAsync(Kernel kernel, ChatHistory history, ILogger logger)
    {
        ChatHistory localHistory =
        [
            new ChatMessageContent(AuthorRole.System, "Analyze the conversation and determine if user input is being solicited. Please respond with a JSON object containing only the following fields: IsRequestingUserInput, IsWorking and Rationale. Fill out the properties in all situations."),
            .. history.TakeLast(1)
        ];

        IChatCompletionService service = kernel.GetRequiredService<IChatCompletionService>();

        ChatMessageContent response = await service.GetChatMessageContentAsync(localHistory);
        var rawText = response.ToString();
        if (string.IsNullOrWhiteSpace(rawText))
        {
            logger.LogError("Response is not valid");
            return new IntentResult(false, true, string.Empty);
        }

        try
        {
            IntentResult intent = JsonSerializer.Deserialize<IntentResult>(response.ToString())!;
            logger.LogTrace("{StepName} Response Intent - {IsRequestingUserInput}: {Rationale}", nameof(ManagerAgentStep), intent.IsRequestingUserInput, intent.Rationale);
            return intent;
            
        }
        catch
        {
            logger.LogError("Response is not valid: {rawText}", rawText);
            return new IntentResult(false, true, string.Empty);
        }
    }

    [DisplayName("IntentResult")]
    [Description("this is the result description")]
    public sealed record IntentResult(
        [property:Description("True if user input is requested or solicited.  Addressing the user with no specific request is False.  Asking a question to the user is True.")]
        bool IsRequestingUserInput,
        [property:Description("True if the user request is being worked on.")]
        bool IsWorking,
        [property:Description("Rationale for the value assigned to IsRequestingUserInput")]
        string Rationale);

    private static IChatHistoryProvider GetHistory(Kernel kernel) =>
        kernel.Services.GetRequiredService<IChatHistoryProvider>();

    private static TAgent GetAgent<TAgent>(Kernel kernel, string key) where TAgent : Agent =>
        kernel.Services.GetRequiredKeyedService<TAgent>(key);

    private static async Task<string> SummarizeHistoryAsync(Kernel kernel, string key, IReadOnlyList<ChatMessageContent> history)
    {
        ChatHistorySummarizationReducer reducer = kernel.Services.GetRequiredKeyedService<ChatHistorySummarizationReducer>(key);
        IEnumerable<ChatMessageContent>? reducedResponse = await reducer.ReduceAsync(history);
        ChatMessageContent summary = reducedResponse?.First() ?? throw new InvalidDataException("No summary available");
        return summary.ToString();
    }
}

public class AgentGroupChatStep : KernelProcessStep
{
    public const string ChatServiceKey = $"{nameof(AgentGroupChatStep)}:{nameof(ChatServiceKey)}";
    public const string ReducerServiceKey = $"{nameof(AgentGroupChatStep)}:{nameof(ReducerServiceKey)}";

    public static class ProcessStepFunctions
    {
        public const string InvokeAgentGroup = nameof(InvokeAgentGroup);
    }

    [KernelFunction(ProcessStepFunctions.InvokeAgentGroup)]
    public async Task InvokeAgentGroupAsync(KernelProcessStepContext context, Kernel kernel, string input)
    {
        AgentGroupChat chat = kernel.GetRequiredService<AgentGroupChat>();

        // Reset chat state from previous invocation
        //await chat.ResetAsync();
        chat.IsComplete = false;

        ChatMessageContent message = new(AuthorRole.User, input);
        chat.AddChatMessage(message);
        await context.EmitEventAsync(new() { Id = AgentOrchestrationEvents.GroupMessage, Data = message });

        await foreach (ChatMessageContent response in chat.InvokeAsync())
        {
           await context.EmitEventAsync(new() { Id = AgentOrchestrationEvents.GroupMessage, Data = response });
        }

        ChatMessageContent[] history = await chat.GetChatMessagesAsync().Reverse().ToArrayAsync();

        // Summarize the group chat as a response to the primary agent
        string summary = await SummarizeHistoryAsync(kernel, ReducerServiceKey, history);

        await context.EmitEventAsync(new() { Id = AgentOrchestrationEvents.GroupCompleted, Data = summary });
    }

    private static async Task<string> SummarizeHistoryAsync(Kernel kernel, string key, IReadOnlyList<ChatMessageContent> history)
    {
        ChatHistorySummarizationReducer reducer = kernel.Services.GetRequiredKeyedService<ChatHistorySummarizationReducer>(key);
        IEnumerable<ChatMessageContent>? reducedResponse = await reducer.ReduceAsync(history);
        ChatMessageContent summary = reducedResponse?.First() ?? throw new InvalidDataException("No summary available");
        return summary.ToString();
    }
}

public static class AgentOrchestrationEvents
{
    public static readonly string StartProcess = nameof(StartProcess);

    public static readonly string AgentResponse = nameof(AgentResponse);
    public static readonly string AgentResponded = nameof(AgentResponded);
    public static readonly string AgentWorking = nameof(AgentWorking);
    public static readonly string GroupInput = nameof(GroupInput);
    public static readonly string GroupMessage = nameof(GroupMessage);
    public static readonly string GroupCompleted = nameof(GroupCompleted);
}

public static class CommonEvents
{
    public static readonly string UserInputReceived = nameof(UserInputReceived);
    public static readonly string UserInputComplete = nameof(UserInputComplete);
    public static readonly string AssistantResponseGenerated = nameof(AssistantResponseGenerated);
    public static readonly string Exit = nameof(Exit);
}

public record FlowParameter(Guid ChatId, string ConnectionId, string Requirement);
