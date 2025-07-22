using MCP.Host.Api;
using MCP.Host.Hubs;
using MCP.Host.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;

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