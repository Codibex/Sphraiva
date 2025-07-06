using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace MCP.Server;

[McpServerResourceType]
public static class FileSystemResource
{
    /// <summary>
    /// Does not work with path parameter in the current implementation of mcp.
    /// Client does not list the method.
    /// </summary>
    /// <returns></returns>
    [McpServerResource()]
    [Description("Returns a list of directories and files for the provided path.")]
    public static TextResourceContents ListDataDirectory()
    {
        var path = "../data";
        var files = Directory.GetFiles(path);
        var dirs = Directory.GetDirectories(path);
        var result = new FileSystemResourceResult(dirs, files);
        return new TextResourceContents
        {
            Text = JsonSerializer.Serialize(result)
        };
    }
}

public record FileSystemResourceResult(IReadOnlyCollection<string> DirectoriesBuilder, IReadOnlyCollection<string> Files);
