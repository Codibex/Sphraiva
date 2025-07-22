using System.ComponentModel;
using System.Text.Json;
using MCP.Host.Api;
using MCP.Host.Hubs;
using MCP.Host.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using static MCP.Host.Agents.GenerateDocumentationStep;

namespace MCP.Host.Agents;

#pragma warning disable SKEXP0080
public class CodingAgentProcess(IKernelProvider kernelProvider, IHubContext<CodeAgentHub, ICodeAgentHub> hubContext)
{
    public async Task RunAsync(CodingAgentImplementationTask implementationTask, CancellationToken cancellationToken)
    {

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
        IExternalKernelProcessMessageChannel myExternalMessageChannel = new CodingAgentProcessMessageChannel(implementationTask.ConnectionId, hubContext);

        var process = processBuilder.Build();
        await process.StartAsync(kernel,
            new KernelProcessEvent
            {
                Id = "StartDocumentation",
                Data = "Contoso GlowBrew"
            },
            myExternalMessageChannel);
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
        3. Provide the feedback as json with the following properties:
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