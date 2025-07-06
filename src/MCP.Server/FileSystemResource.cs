using ModelContextProtocol.Server;

namespace MCP.Server;

[McpServerResourceType]
public static class FileSystemResource
{
    [McpServerResource]
    public static Task<object?> ListDirectory(dynamic parameters)
    {
        string? path = parameters?.path as string ?? ".";
        var files = Directory.GetFiles(path);
        var dirs = Directory.GetDirectories(path);
        return Task.FromResult<object?>(new { files, dirs });
    }

}