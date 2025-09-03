using Microsoft.SemanticKernel;

namespace MCP.Host.Agents.CodingAgent.Steps;

public class GatherRequirementStep : KernelProcessStep
{
    public static class ProcessStepFunctions
    {
        public const string START_REQUIREMENT_IMPLEMENTATION = nameof(START_REQUIREMENT_IMPLEMENTATION);
    }
    
    [KernelFunction]
    public string GatherRequirementData(Kernel kernel, string requirement)
    {
        var logger = kernel.GetRequiredService<ILogger<InputCheckStep>>();
        logger.LogInformation("Gathering requirement data");
        return requirement;
    }
}