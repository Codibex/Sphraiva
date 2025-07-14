namespace MCP.Server.Services.FileSystem;

public record DirectoryListInfo(IEnumerable<string> Directories, IEnumerable<string> Files);