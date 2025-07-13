using System.ComponentModel;

namespace MCP.Server.Results;

[Description("Represents the result of listing the contents of a directory in the server's data directory.")]
public record ListDirectoryResult(
    [Description("Names of all subdirectories in the specified directory.")] 
    IEnumerable<string> Directories,
    [Description("Names of all files in the specified directory.")] 
    IEnumerable<string> Files
);
