namespace MCP.Host.Agents.CodingAgent.Prompts;

public record PromptBase(
    string ManagerAgentInstructions,
    string AnalysisAgentInstructions,
    string ImplementationAgentInstructions,
    string SelectionFunction,
    string TerminationFunction)
{
}
