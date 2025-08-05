using Microsoft.SemanticKernel;

namespace MCP.Host.Services;

public interface IKernelFactory
{
    Kernel Create(bool withPlugins = true);
}