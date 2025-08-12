using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

namespace MCP.Host.Services;

public interface IKernelFactory
{
    Kernel Create(bool withPlugins = true);
    Kernel CreateAgentGroupChatKernel(ChatCompletionAgent managerAgent, AgentGroupChat chat);
}