using System.ComponentModel;

namespace MCP.Server.Results;

[Description("Represents the result of reading a file from the server's data directory.")]
public record ReadFileResult(
    [Description("The name of the file that was read.")] 
    string FileName, 
    [Description("The full text content of the file.")] 
    string Content);