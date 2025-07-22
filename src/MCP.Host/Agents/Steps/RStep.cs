using Microsoft.SemanticKernel;

namespace MCP.Host.Agents.Steps;

public class RStep : KernelProcessStep
{
    [KernelFunction]
    public string RFunction(InputCheckResult input)
    {
        return input.Requirement;
    }
}