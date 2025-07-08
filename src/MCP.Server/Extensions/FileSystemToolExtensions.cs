using System.Diagnostics.CodeAnalysis;
using ModelContextProtocol.Server;

namespace MCP.Server.Extensions;

public static class FileSystemToolExtensions
{
    public static bool TryGetService<TService>(this IMcpServer server, [NotNullWhen(true)] out TService? service)
        where TService : notnull
    {
        using var scope = server.Services?.CreateScope();
        if (scope is null)
        {
            service = default;
            return false;
        }
        
        service = scope.ServiceProvider.GetRequiredService<TService>();
        return true;
    }
}