using Microsoft.SemanticKernel;

namespace MCP.Host.Services;

public interface IKernelProvider
{
    Task<Kernel> GetAsync();
}