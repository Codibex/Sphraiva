using Microsoft.SemanticKernel;

namespace MCP.Host.Services;

public interface IKernelProvider
{
    Kernel Get();
}