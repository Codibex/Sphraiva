using MCP.Host.Agents.CodingAgent.Events;
using MCP.Host.Agents.CodingAgent.Prompts;
using MCP.Host.Agents.CodingAgent.Steps;
using MCP.Host.Hubs;
using MCP.Host.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace MCP.Host.Agents.CodingAgent;

public class CodingAgentWorkflow(IKernelFactory kernelFactory, IHubContext<CodingAgentHub, ICodingAgentHub> hubContext, ILoggerFactory loggerFactory)
{
    public async Task RunAsync(WorkflowParameter parameter)
    {
        // Plugin parameter can be false and added for specific agents
        var kernel = kernelFactory.Create(true);

        var prompt = new Prompt_Qwen3_14b();

        var managerAgent = CreateAgent(AgentNames.MANAGER_AGENT_NAME, prompt.ManagerAgentInstructions, kernel.Clone());
        var analysisAgent = CreateAgent(AgentNames.ANALYSIS_AGENT_NAME, prompt.AnalysisAgentInstructions, kernel.Clone());
        var implementationAgent = CreateAgent(AgentNames.IMPLEMENTATION_AGENT_NAME, prompt.ImplementationAgentInstructions, kernel.Clone());

        var selectionFunction = AgentGroupChat.CreatePromptFunctionForStrategy(prompt.SelectionFunction);
        var terminationFunction = AgentGroupChat.CreatePromptFunctionForStrategy(prompt.TerminationFunction);

        var chat = new AgentGroupChat(analysisAgent, implementationAgent)
        {
            ExecutionSettings = new AgentGroupChatSettings
            {
                SelectionStrategy = new KernelFunctionSelectionStrategy(selectionFunction, kernel)
                {
                    HistoryVariableName = "history",
                    ResultParser = (r) =>
                    {
                        var agent = r.GetValue<string>() ?? AgentNames.ANALYSIS_AGENT_NAME;
                        return agent;
                    },
                    InitialAgent = analysisAgent
                },
                TerminationStrategy = new KernelFunctionTerminationStrategy(terminationFunction, kernel)
                {
                    HistoryVariableName = "history",
                    ResultParser = (r) =>
                    {
                        var result = r.GetValue<string>()
                                         ?.Trim()
                                         .Equals("workflow completed", StringComparison.InvariantCultureIgnoreCase) ??
                                     false;

                        return result;
                    },
                    MaximumIterations = 1000
                }
            },
            LoggerFactory = loggerFactory
        };

        var kernel2 = kernelFactory.CreateAgentGroupChatKernel(managerAgent, chat);
        
        var process = SetupProcess(parameter.ChatId);
        IExternalKernelProcessMessageChannel processMessageChannel = new CodingAgentWorkflowMessageChannel(parameter.ConnectionId, hubContext);
        
        await process.StartAsync(kernel2,
            new KernelProcessEvent
            {
                Id = GatherRequirementStep.ProcessStepFunctions.START_REQUIREMENT_IMPLEMENTATION,
                Data = parameter.Requirement
            },
            processMessageChannel);
    }

    private KernelProcess SetupProcess(Guid chatId)
    {
        ProcessBuilder processBuilder = new($"coding-agent-{chatId}");

        var gatherRequirementStep = processBuilder.AddStepFromType<GatherRequirementStep>();
        var inputCheckStep = processBuilder.AddStepFromType<InputCheckStep>();
        var setupInfrastructureStep = processBuilder.AddStepFromType<SetupInfrastructureStep>();

        var managerAgentStep = processBuilder.AddStepFromType<ManagerAgentStep>();
        var agentGroupStep = processBuilder.AddStepFromType<AgentGroupChatStep>();

        var proxyStep = processBuilder.AddProxyStep("codingAgentWorkflowProxy", [CodingAgentWorkflowTopics.REQUEST_REQUIREMENT_UPDATE, CodingAgentWorkflowTopics.WORKFLOW_UPDATE, CodingAgentWorkflowTopics.SETUP_INFRASTRUCTURE_FAILED, "PublishDocumentation"]);

        processBuilder
            .OnInputEvent(GatherRequirementStep.ProcessStepFunctions.START_REQUIREMENT_IMPLEMENTATION)
            .SendEventTo(new(gatherRequirementStep));

        // Hooking up the process steps
        gatherRequirementStep
            .OnFunctionResult()
            .SendEventTo(new ProcessFunctionTargetBuilder(inputCheckStep, functionName: InputCheckStep.ProcessStepFunctions.CHECK_INPUT));

        inputCheckStep
            .OnEvent(InputCheckStep.OutputEvents.INPUT_VALIDATION_FAILED)
            .EmitExternalEvent(proxyStep, CodingAgentWorkflowTopics.REQUEST_REQUIREMENT_UPDATE)
            .StopProcess();

        inputCheckStep
            .OnEvent(InputCheckStep.OutputEvents.INPUT_VALIDATION_SUCCEEDED)
            .SendEventTo(new ProcessFunctionTargetBuilder(setupInfrastructureStep));

        setupInfrastructureStep
            .OnEvent(SetupInfrastructureStep.OutputEvents.SETUP_INFRASTRUCTURE_SUCCEEDED)
            .SendEventTo(new ProcessFunctionTargetBuilder(managerAgentStep,
                ManagerAgentStep.ProcessStepFunctions.INVOKE_AGENT));

        setupInfrastructureStep
            .OnEvent(SetupInfrastructureStep.OutputEvents.SETUP_INFRASTRUCTURE_FAILED)
            .EmitExternalEvent(proxyStep, CodingAgentWorkflowTopics.SETUP_INFRASTRUCTURE_FAILED)
            .StopProcess();

        // Delegate to inner agents
        managerAgentStep
            .OnEvent(AgentOrchestrationEvents.AgentWorking)
            .SendEventTo(new ProcessFunctionTargetBuilder(managerAgentStep, ManagerAgentStep.ProcessStepFunctions.INVOKE_GROUP));

        // Provide input to inner agents
        managerAgentStep
            .OnEvent(AgentOrchestrationEvents.GroupInput)
            .SendEventTo(new ProcessFunctionTargetBuilder(agentGroupStep, parameterName: "input"));

        // Provide group messages to public hub
        agentGroupStep
            .OnEvent(AgentOrchestrationEvents.GroupMessage)
            .EmitExternalEvent(proxyStep, CodingAgentWorkflowTopics.WORKFLOW_UPDATE);

        // Provide inner response to primary agent
        agentGroupStep
            .OnEvent(AgentOrchestrationEvents.GroupCompleted)
            .SendEventTo(new ProcessFunctionTargetBuilder(managerAgentStep, ManagerAgentStep.ProcessStepFunctions.RECEIVE_RESPONSE, parameterName: "response"));

        return processBuilder.Build();
    }

    private ChatCompletionAgent CreateAgent(string agentName, string instructions, Kernel kernel) =>
        new()
        {
            Name = agentName,
            Instructions = instructions,
            Kernel = kernel,
            LoggerFactory = loggerFactory,
            Arguments = new KernelArguments(
                new OllamaPromptExecutionSettings
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                    Temperature = 0
                })
        };
}