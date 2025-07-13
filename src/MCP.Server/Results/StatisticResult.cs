using System.ComponentModel;

namespace MCP.Server.Results;

[Description("Represents the metadata and status of a file or directory in the server's data directory.")]
public record StatisticResult(
    [Description("True if the path exists, otherwise false.")]
    bool Exists,
    [Description("The type of the resource: 'File' or 'Directory'.")]
    string Type,
    [Description("Size in bytes. For files, this is the file size; for directories, this may be not set.")] 
    long? Size,
    [Description("Creation date and time of the resource.")]
    DateTime Created,
    [Description("Last modification date and time of the resource.")]
    DateTime LastModified
);
