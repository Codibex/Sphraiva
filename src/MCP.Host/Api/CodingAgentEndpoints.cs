using MCP.Host.Agents;
using MCP.Host.Chat;
using MCP.Host.Contracts;
using MCP.Host.Plugins;
using MCP.Host.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace MCP.Host.Api;

public static class CodingAgentEndpoints
{
    private const string ManagerInstructions =
        """
        Capture information provided by the user for their scheduling request.
        Request confirmation without suggesting additional details.
        Once confirmed inform them you're working on the request.
        Never provide a direct answer to the user's request.
        """;

    private const string InfrastructureInstructions =
        """
        You are responsible for all infrastructure-related tasks in the development workflow.
        Create, start, and remove development containers as needed.
        Clone repositories and prepare the environment for implementation.
        Do not implement code changes or review code.
        """;

    private const string ChangeRequestInstructions =
        """
        You are an expert in analyzing user requirements and change requests for software projects.
        Your task is to identify what needs to be changed, added, or removed in the codebase based on the user's description.
        Ask clarifying questions if information is missing.
        Summarize the required changes in clear, actionable steps.
        Do not provide implementation details or code.
        """;

    private const string ImplementationInstructions =
        """
        You are an expert software developer focused on implementing the requested changes.
        Your task is to apply the change plan step by step, writing clean, maintainable code.
        Only address topics relevant to the implementation.
        If requirements are unclear, ask for clarification.
        Before pushing changes, ensure the solution builds successfully and all requirements are met.
        After implementation, commit and push the changes to the remote repository.
        Do not review or critique code; focus on delivering the requested changes.
        """;

    private const string ReviewInstructions =
        """
        You are a senior code reviewer responsible for ensuring code quality and correctness.
        Review the implemented changes for bugs, maintainability, and adherence to best practices.
        Provide constructive feedback and suggest improvements if necessary.
        Do not implement changes; only review and comment.
        """;

    public static void MapCodingAgentEndpoints(this WebApplication app)
    {
        app.MapPost("/agent/workflow/approve",
            async (HeaderValueProvider headerValueProvider, ICodingAgentProcessStore store, CodingAgentImplementationApprovalRequest request) =>
            {
                var chatId = headerValueProvider.ChatId;
                if (store.TryGetProcess(chatId!.Value, out var process))
                {
                    await process.UserApprovedDocumentAsync(request.Approve);
                }
            }
        ).AddEndpointFilter<RequireChatIdEndpointFilter>();

        app.MapPost("/agent/workflow",
                (
                    HeaderValueProvider headerValueProvider,
                    CodingAgentImplementationRequest request,
                    ICodingAgentChannel channel) =>
                {
                    var codingAgentHubConnectionId = headerValueProvider.CodingAgentHubConnectionId;
                    var chatId = headerValueProvider.ChatId;

                    channel.AddTask(new CodingAgentImplementationTask(chatId!.Value, codingAgentHubConnectionId!,
                        request.Requirement));
                    return Results.Ok("Starting Implementation");
                    // Create the process builder
                    //ProcessBuilder processBuilder = new("DocumentationGeneration");

                    //// Add the steps
                    //var infoGatheringStep = processBuilder.AddStepFromType<GatherProductInfoStep>();
                    //var docsGenerationStep = processBuilder.AddStepFromType<GenerateDocumentationStep>();
                    //var docsProofreadStep = processBuilder.AddStepFromType<ProofreadStep>();
                    //var docsPublishStep = processBuilder.AddStepFromType<PublishDocumentationStep>();

                    //var proxyStep = processBuilder.AddProxyStep("workflowProxy", ["RequestUserReview", "PublishDocumentation"]);

                    //// Orchestrate the events
                    //processBuilder
                    //    .OnInputEvent("StartDocumentation")
                    //    .SendEventTo(new(infoGatheringStep));

                    //processBuilder
                    //    .OnInputEvent("UserRejectedDocument")
                    //    .SendEventTo(new(docsGenerationStep, functionName: "ApplySuggestions"));

                    //processBuilder
                    //    .OnInputEvent("UserApprovedDocument")
                    //    .SendEventTo(new(docsPublishStep, parameterName: "userApproval"));

                    //infoGatheringStep
                    //    .OnFunctionResult()
                    //    .SendEventTo(new ProcessFunctionTargetBuilder(docsGenerationStep, functionName: "GenerateDocumentation"));

                    //docsGenerationStep
                    //    .OnEvent("DocumentationGenerated")
                    //    .SendEventTo(new ProcessFunctionTargetBuilder(docsProofreadStep));

                    //docsProofreadStep
                    //    .OnEvent("DocumentationRejected")
                    //    .SendEventTo(new ProcessFunctionTargetBuilder(docsGenerationStep, functionName: "ApplySuggestions"));

                    //docsProofreadStep
                    //    .OnEvent("DocumentationApproved")
                    //    .EmitExternalEvent(proxyStep, "RequestUserReview")
                    //    .SendEventTo(new ProcessFunctionTargetBuilder(docsPublishStep));

                    //docsPublishStep
                    //    .OnFunctionResult()
                    //    .EmitExternalEvent(proxyStep, "PublishDocumentation");

                    //var kernel = kernelProvider.Get();
                    //IExternalKernelProcessMessageChannel myExternalMessageChannel = new CodingAgentProcessMessageChannel(hubContext);

                    //// Build and run the process
                    //var process = processBuilder.Build();
                    //await process.StartAsync(kernel,
                    //    new KernelProcessEvent
                    //    {
                    //        Id = "StartDocumentation",
                    //        Data = "Contoso GlowBrew"
                    //    },
                    //    myExternalMessageChannel);

                    //            response.ContentType = MediaTypeNames.Text.EventStream;

                    //            var chatId = headerValueProvider.ChatId!.Value;
                    //            var thread = chatCache.GetOrCreateThread(chatId);

                    //
                    //            var processBuilder = new ProcessBuilder("CodingProcess");

                    //            KernelProcess kernelProcess = processBuilder.Build();

                    //            var builder = kernelProvider.GetBuilder();

                    //ChatCompletionAgent managerAgent = CreateAgent("Manager", ManagerInstructions, kernelProvider.Get());
                    //builder.Services.AddKeyedSingleton(ManagerAgentStep.AgentServiceKey, managerAgent);

                    // Create and inject group chat into service collection
                    //SetupGroupChat(builder, builder.Build(), pluginCache);

                    //builder.Services.AddKeyedSingleton(ManagerAgentStep.ReducerServiceKey, SetupReducer(kernel, ManagerSummaryInstructions));
                    //builder.Services.AddKeyedSingleton(AgentGroupChatStep.ReducerServiceKey, SetupReducer(kernel, SuggestionSummaryInstructions));

                    //var kernel = builder.Build();

                    //await using LocalKernelProcessContext localProcess =
                    //    await kernelProcess.StartAsync(
                    //        kernel,
                    //        new KernelProcessEvent()
                    //        {
                    //            Id = AgentOrchestrationEvents.StartProcess,
                    //            Data = request.Message
                    //        });

                    //foreach (ChatMessageContent message in thread.ChatHistory)
                    //{
                    //    var content = message.Content ?? string.Empty;

                    //    await response.WriteAsync(content, cancellationToken);
                    //    await response.Body.FlushAsync(cancellationToken);
                    //}

                    //void AttachErrorStep(ProcessStepBuilder step, params string[] functionNames)
                    //{
                    //    foreach (string functionName in functionNames)
                    //    {
                    //        step
                    //            .OnFunctionError(functionName)
                    //            .SendEventTo(new ProcessFunctionTargetBuilder(renderMessageStep, RenderMessageStep.ProcessStepFunctions.RenderError, "error"))
                    //            .StopProcess();
                    //    }
                    //}
                })
            .AddEndpointFilter<RequireChatIdEndpointFilter>()
            .AddEndpointFilter<RequireCodingAgentHubConnectionIdEndpointFilter>();
    }

    private static ChatCompletionAgent CreateAgent(string name, string instructions, Kernel kernel) =>
        new()
        {
            Name = name,
            Instructions = instructions,
            Kernel = kernel.Clone(),
            Arguments =
                new KernelArguments(
                    new OpenAIPromptExecutionSettings
                    {
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                        Temperature = 0,
                    }),
        };

    private static void SetupGroupChat(IKernelBuilder builder, Kernel kernel, IMcpPluginCache pluginCache)
    {
        const string InfrastructureAgentName = "InfrastructureAgent";
        ChatCompletionAgent infrastructureAgent = CreateAgent(InfrastructureAgentName, InfrastructureInstructions, kernel.Clone());
        var tools = pluginCache.GetToolsForPlugin(PluginNames.Sphraiva);
        infrastructureAgent.Kernel.Plugins.AddFromFunctions(PluginNames.Sphraiva, tools.Select(t => t.AsKernelFunction()));

        //infrastructureAgent.Kernel.Plugins.AddFromType<CalendarPlugin>();

        const string ChangeRequestAgentName = "ChangeRequestAgent";
        ChatCompletionAgent changeRequestAgent = CreateAgent(ChangeRequestAgentName, ChangeRequestInstructions, kernel.Clone());
        changeRequestAgent.Kernel.Plugins.AddFromFunctions(PluginNames.Sphraiva, tools.Select(t => t.AsKernelFunction()));

        const string ImplementationAgentName = "ImplementationAgent";
        ChatCompletionAgent implementationAgent = CreateAgent(ImplementationAgentName, ImplementationInstructions, kernel.Clone());
        implementationAgent.Kernel.Plugins.AddFromFunctions(PluginNames.Sphraiva, tools.Select(t => t.AsKernelFunction()));

        const string ReviewAgentName = "ReviewAgent";
        ChatCompletionAgent reviewAgent = CreateAgent(ReviewAgentName, ReviewInstructions, kernel.Clone());
        reviewAgent.Kernel.Plugins.AddFromFunctions(PluginNames.Sphraiva, tools.Select(t => t.AsKernelFunction()));

        KernelFunction selectionFunction =
#pragma warning disable SKEXP0110
            AgentGroupChat.CreatePromptFunctionForStrategy(

                $$$"""
                   Decide which agent should act next based on the last message and the current workflow step.
                   State only the name of the participant to take the next turn.

                   Choose only from these participants:
                   - {{{InfrastructureAgentName}}}
                   - {{{ChangeRequestAgentName}}}
                   - {{{ImplementationAgentName}}}
                   - {{{ReviewAgentName}}}

                   Rules:
                   - Start with {{{InfrastructureAgentName}}} for environment setup.
                   - After infrastructure is ready, {{{ChangeRequestAgentName}}} analyzes requirements.
                   - {{{ImplementationAgentName}}} implements the changes after analysis.
                   - {{{ReviewAgentName}}} reviews the code after implementation.
                   - Repeat as needed if feedback is given.
             
                   History:
                   {{$history}}
                   """,
                safeParameterNames: "history");

        KernelFunction terminationFunction =
            AgentGroupChat.CreatePromptFunctionForStrategy(
                $$$"""
                Decide if the coding workflow is complete.
                The workflow is only complete if:
                - The infrastructure is set up and cleaned up.
                - All required changes are detected and implemented.
                - The code has been reviewed and all review feedback is addressed.
                - The changes are successfully built and pushed to the remote repository.
                
                If all these conditions are met, respond with a single word: yes.
                Otherwise, respond with: no.

                History:
                {{$history}}
                """,
                safeParameterNames: "history");

        AgentGroupChat chat =
            new(infrastructureAgent, changeRequestAgent, implementationAgent, reviewAgent)
            {
                // NOTE: Replace logger when using outside of sample.
                // Use `this.LoggerFactory` to observe logging output as part of sample.
                LoggerFactory = NullLoggerFactory.Instance,
                ExecutionSettings = new()
                {
                    SelectionStrategy =
                        new KernelFunctionSelectionStrategy(selectionFunction, kernel)
                        {
                            HistoryVariableName = "history",
                            HistoryReducer = new ChatHistoryTruncationReducer(1),
                            ResultParser = (result) => result.GetValue<string>() ?? changeRequestAgent.Name!,
                        },
                    TerminationStrategy =
                        new KernelFunctionTerminationStrategy(terminationFunction, kernel)
                        {
                            HistoryVariableName = "history",
                            MaximumIterations = 12,
                            //HistoryReducer = new ChatHistoryTruncationReducer(2),
                            ResultParser = (result) => result.GetValue<string>()?.Contains("yes", StringComparison.OrdinalIgnoreCase) ?? false,
                        }
                }
            };
        builder.Services.AddSingleton(chat);
    }
}