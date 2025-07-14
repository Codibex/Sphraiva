using System.ComponentModel;

namespace MCP.Server.Results;

[Description(
    """
    Represents the result of an existence check for a file or directory in the server's data directory.
    """
)]
public record ExistsResult(
    [Description("True if the diretory or file exists, otherwise false.")] bool Exists,
    [Description("The type of the resource: 'File', 'Directory' or 'Unknown'.")] string Type
);
