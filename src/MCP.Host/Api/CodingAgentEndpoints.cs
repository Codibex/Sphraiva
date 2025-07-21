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
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.SemanticKernel.Connectors.Ollama;
using static MCP.Host.Api.GenerateDocumentationStep;

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

#pragma warning disable SKEXP0080
    public static void MapCodingAgentEndpoints(this WebApplication app)
    {
        app.MapPost("/agent/workflow", async (HeaderValueProvider headerValueProvider, ChatRequest request, IKernelProvider kernelProvider, IMcpPluginCache pluginCache, ChatCache chatCache, HttpResponse response, CancellationToken cancellationToken) =>
        {
            response.ContentType = MediaTypeNames.Text.EventStream;

            // Create the process builder
            ProcessBuilder processBuilder = new("DocumentationGeneration");

            // Add the steps
            var infoGatheringStep = processBuilder.AddStepFromType<GatherProductInfoStep>();
            var docsGenerationStep = processBuilder.AddStepFromType<GenerateDocumentationStep>();
            var docsProofreadStep = processBuilder.AddStepFromType<ProofreadStep>();
            var docsPublishStep = processBuilder.AddStepFromType<PublishDocumentationStep>();

            var proxyStep = processBuilder.AddProxyStep("workflowProxy", ["RequestUserReview", "PublishDocumentation"]);

            // Orchestrate the events
            processBuilder
                .OnInputEvent("StartDocumentation")
                .SendEventTo(new(infoGatheringStep));

            processBuilder
                .OnInputEvent("UserRejectedDocument")
                .SendEventTo(new(docsGenerationStep, functionName: "ApplySuggestions"));

            processBuilder
                .OnInputEvent("UserApprovedDocument")
                .SendEventTo(new(docsPublishStep, parameterName: "userApproval"));

            infoGatheringStep
                .OnFunctionResult()
                .SendEventTo(new ProcessFunctionTargetBuilder(docsGenerationStep, functionName: "GenerateDocumentation"));

            docsGenerationStep
                .OnEvent("DocumentationGenerated")
                .SendEventTo(new ProcessFunctionTargetBuilder(docsProofreadStep));

            docsProofreadStep
                .OnEvent("DocumentationRejected")
                .SendEventTo(new ProcessFunctionTargetBuilder(docsGenerationStep, functionName: "ApplySuggestions"));

            docsProofreadStep
                .OnEvent("DocumentationApproved")
                .EmitExternalEvent(proxyStep, "RequestUserReview")
                .SendEventTo(new ProcessFunctionTargetBuilder(docsPublishStep));

            docsPublishStep
                .OnFunctionResult()
                .EmitExternalEvent(proxyStep, "PublishDocumentation");

            var kernel = kernelProvider.Get();
            IExternalKernelProcessMessageChannel myExternalMessageChannel = new MyCloudEventClient(response);

            // Build and run the process
            var process = processBuilder.Build();
            await process.StartAsync(kernel,
                new KernelProcessEvent
                {
                    Id = "StartDocumentation",
                    Data = "Contoso GlowBrew"
                },
                myExternalMessageChannel);

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
        .AddEndpointFilter<RequireChatIdEndpointFilter>();
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

/*
 * This is the new region
 */

public class GatherProductInfoStep : KernelProcessStep
{
    [KernelFunction]
    public string GatherProductInformation(string productName)
    {
        Console.WriteLine($"{nameof(GatherProductInfoStep)}:\n\tGathering product information for product named {productName}");

        // For example purposes we just return some fictional information.
        return
            """
            Product Description:
            GlowBrew is a revolutionary AI driven coffee machine with industry leading number of LEDs and programmable light shows. The machine is also capable of brewing coffee and has a built in grinder.

            Product Features:
            1. **Luminous Brew Technology**: Customize your morning ambiance with programmable LED lights that sync with your brewing process.
            2. **AI Taste Assistant**: Learns your taste preferences over time and suggests new brew combinations to explore.
            3. **Gourmet Aroma Diffusion**: Built-in aroma diffusers enhance your coffee's scent profile, energizing your senses before the first sip.

            Troubleshooting:
            - **Issue**: LED Lights Malfunctioning
                - **Solution**: Reset the lighting settings via the app. Ensure the LED connections inside the GlowBrew are secure. Perform a factory reset if necessary.
            """;
    }
}

// A process step to generate documentation for a product
public class GenerateDocumentationStep : KernelProcessStep<GeneratedDocumentationState>
{
    private GeneratedDocumentationState _state = new();

    private string systemPrompt =
            """
            Your job is to write high quality and engaging customer facing documentation for a new product from Contoso. You will be provide with information
            about the product in the form of internal documentation, specs, and troubleshooting guides and you must use this information and
            nothing else to generate the documentation. If suggestions are provided on the documentation you create, take the suggestions into account and
            rewrite the documentation. Make sure the product sounds amazing.
            """;

    // Called by the process runtime when the step instance is activated. Use this to load state that may be persisted from previous activations.
    public override ValueTask ActivateAsync(KernelProcessStepState<GeneratedDocumentationState> state)
    {
        this._state = state.State!;
        this._state.ChatHistory ??= new ChatHistory(systemPrompt);

        return base.ActivateAsync(state);
    }

    [KernelFunction]
    public async Task GenerateDocumentationAsync(Kernel kernel, KernelProcessStepContext context, string productInfo)
    {
        Console.WriteLine($"{nameof(GenerateDocumentationStep)}:\n\tGenerating documentation for provided productInfo...");

        // Add the new product info to the chat history
        this._state.ChatHistory!.AddUserMessage($"Product Info:\n\n{productInfo}");

        // Get a response from the LLM
        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var generatedDocumentationResponse = await chatCompletionService.GetChatMessageContentAsync(this._state.ChatHistory!);

        await context.EmitEventAsync("DocumentationGenerated", generatedDocumentationResponse.Content!.ToString());
    }

    [KernelFunction]
    public async Task ApplySuggestionsAsync(Kernel kernel, KernelProcessStepContext context, string suggestions)
    {
        Console.WriteLine($"{nameof(GenerateDocumentationStep)}:\n\tRewriting documentation with provided suggestions...");

        // Add the new product info to the chat history
        this._state.ChatHistory!.AddUserMessage($"Rewrite the documentation with the following suggestions:\n\n{suggestions}");

        // Get a response from the LLM
        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var generatedDocumentationResponse = await chatCompletionService.GetChatMessageContentAsync(this._state.ChatHistory!);

        await context.EmitEventAsync("DocumentationGenerated", generatedDocumentationResponse.Content!.ToString());
    }

    public class GeneratedDocumentationState
    {
        public ChatHistory? ChatHistory { get; set; }
    }
}

// A process step to publish documentation
public class PublishDocumentationStep : KernelProcessStep
{
    [KernelFunction]
    public DocumentInfo PublishDocumentation(DocumentInfo document, bool userApproval)
    {
        if (userApproval)
            // For example purposes we just write the generated docs to the console
        {
            Console.WriteLine($"[{nameof(PublishDocumentationStep)}]:\tPublishing product documentation approved by user: \n{document.Title}\n{document.Content}");
        }
        return document;
    }
}

// Custom classes must be serializable
public class DocumentInfo
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

// A process step to proofread documentation
public class ProofreadStep : KernelProcessStep
{
    [KernelFunction]
    public async Task ProofreadDocumentationAsync(Kernel kernel, KernelProcessStepContext context, string documentation)
    {
        Console.WriteLine($"{nameof(ProofreadDocumentationAsync)}:\n\tProofreading documentation...");

        var systemPrompt =
            """
        Your job is to proofread customer facing documentation for a new product from Contoso. You will be provide with proposed documentation
        for a product and you must do the following things:

        1. Determine if the documentation is passes the following criteria:
            1. Documentation must use a professional tone.
            1. Documentation should be free of spelling or grammar mistakes.
            1. Documentation should be free of any offensive or inappropriate language.
            1. Documentation should be technically accurate.
        2. If the documentation does not pass 1, you must write detailed feedback of the changes that are needed to improve the documentation. 
        3. Provide the feedback in a json format with the following properties:
           1. bool MeetsExpectations
           1. string Explanation
           1. List<string> Suggestions
        """;

        ChatHistory chatHistory = new ChatHistory(systemPrompt);
        chatHistory.AddUserMessage(documentation);

        // Use structured output to ensure the response format is easily parsable
        OllamaPromptExecutionSettings settings = new OllamaPromptExecutionSettings();
        
        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var proofreadResponse = await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings: settings);
        try
        {
            var formattedResponse = JsonSerializer.Deserialize<ProofreadingResponse>(proofreadResponse.Content!.ToString());
            Console.WriteLine($"\n\tGrade: {(formattedResponse!.MeetsExpectations ? "Pass" : "Fail")}\n\tExplanation: {formattedResponse.Explanation}\n\tSuggestions: {string.Join("\n\t\t", formattedResponse.Suggestions)}");

            if (formattedResponse.MeetsExpectations)
            {
                await context.EmitEventAsync("DocumentationApproved", data: documentation, visibility: KernelProcessEventVisibility.Public);
            }
            else
            {
                await context.EmitEventAsync("DocumentationRejected", data: new { Explanation = formattedResponse.Explanation, Suggestions = formattedResponse.Suggestions });
            }
        }
        catch
        {
            // Hack to see the code in action because ollama has no response format setting.
            await context.EmitEventAsync("DocumentationApproved", data: documentation);
        }
    }

    // A class 
    private class ProofreadingResponse
    {
        [Description("Specifies if the proposed documentation meets the expected standards for publishing.")]
        public bool MeetsExpectations { get; set; }

        [Description("An explanation of why the documentation does or does not meet expectations.")]
        public string Explanation { get; set; } = "";

        [Description("A lis of suggestions, may be empty if there no suggestions for improvement.")]
        public List<string> Suggestions { get; set; } = new();
    }
}

public class MyCloudEventClient(HttpResponse response) : IExternalKernelProcessMessageChannel
{
    //private MyCustomClient? _customClient;

    // Example of an implementation for the process
    public ValueTask Initialize()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask Uninitialize()
    {
        return ValueTask.CompletedTask;
    }

    public async Task EmitExternalEventAsync(string externalTopicEvent, KernelProcessProxyMessage message)
    {
        switch (externalTopicEvent)
        {
            case "RequestUserReview": 
                var requestDocument = message.EventData?.ToObject() as DocumentInfo;
                if(requestDocument != null)
                {
                    // As an example only writing the request document to the response
                    await response.WriteAsync($"Requesting user review for document: {requestDocument}"); 
                    await response.Body.FlushAsync();
                }

                break;
            case "PublishDocumentation":
                var publishedDocument = message.EventData?.ToObject() as DocumentInfo;
                if (publishedDocument != null)
                {
                    // As an example only writing the request document to the response
                    await response.WriteAsync($"Requesting user review for document: {publishedDocument}");
                    await response.Body.FlushAsync();
                }

                break;
        }

        //// logic used for emitting messages externally.
        //// Since all topics are received here potentially 
        //// some if else/switch logic is needed to map correctly topics with external APIs/endpoints.
        //if (this._customClient != null)
        //{
        //    switch (externalTopicEvent)
        //    {
        //        case "RequestUserReview":
        //            var requestDocument = message.EventData.ToObject() as DocumentInfo;
        //            // As an example only invoking a sample of a custom client with a different endpoint/api route
        //            this._customClient.InvokeAsync("REQUEST_USER_REVIEW", requestDocument);
        //            return;

        //        case "PublishDocumentation":
        //            var publishedDocument = message.EventData.ToObject() as DocumentInfo;
        //            // As an example only invoking a sample of a custom client with a different endpoint/api route
        //            this._customClient.InvokeAsync("PUBLISH_DOC_EXTERNALLY", publishedDocument);
        //            return;
        //    }
        //}
    }

    //public async ValueTask Initialize()
    //{
    //    // logic needed to initialize proxy step, can be used to initialize custom client
    //    this._customClient = new MyCustomClient("http://localhost:8080");
    //    this._customClient.Initialize();
    //}

    //public async ValueTask Uninitialize()
    //{
    //    // Cleanup to be executed when proxy step is uninitialized
    //    if (this._customClient != null)
    //    {
    //        await this._customClient.ShutdownAsync();
    //    }
    //}
}