using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace MCP.Server.Resources;

[McpServerResourceType]
public static class FileSystemResource
{
    /// <summary>
    /// Template resource for listing files and directories in a specified path.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    [McpServerResource()]
    [Description("Returns a list of directories and files for the provided path.")]
    public static TextResourceContents ListDataDirectory(string path)
    {
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
