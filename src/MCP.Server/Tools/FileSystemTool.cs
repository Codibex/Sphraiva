//using MCP.Server.Results;
//using MCP.Server.Services.FileSystem;
//using ModelContextProtocol.Server;
//using System.ComponentModel;

//namespace MCP.Server.Tools;

//[McpServerToolType]
//[Description(
//    """
//    Provides basic file and directory operations (read, write, delete, list, etc.) 
//    within the server's data directory for use by agents and tools. 
//    Not intended for use inside Docker development containers due to potential file system mapping issues.
//    """
//)]
//public class FileSystemTool(IFileSystemService fileSystemService)
//{
//    [McpServerTool(Title = "Read file relative to the server's default directory.", Destructive = false, Idempotent = true, ReadOnly = true, UseStructuredContent = true)]
//    public async Task<ReadFileResult> ReadFileAsync(string file) 
//        => await fileSystemService.ReadFileAsync(file);

//    [McpServerTool(Title = "Write file relative to the server's default directory.", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
//    public async Task<string> WriteFileAsync(string fullFilePath, string content) 
//        => await fileSystemService.WriteFileAsync(fullFilePath, content);

//    [McpServerTool(Title = "Delete file relative to the server's default directory.", Destructive = true, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
//    public string DeleteFile(string fullFilePath)
//        => fileSystemService.DeleteFile(fullFilePath);

//    [McpServerTool(Title = "List directory relative to the server's default directory.", Destructive = false, Idempotent = true, ReadOnly = true, UseStructuredContent = true)]
//    public ListDirectoryResult ListDirectory(string directory) 
//        => fileSystemService.ListDirectory(directory);

//    [McpServerTool(Title = "Create directory relative to the server's default directory.", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
//    public string CreateDirectory(string fullDirectoryPath
//    ) => fileSystemService.CreateDirectory(fullDirectoryPath);

//    [McpServerTool(Title = "Delete directory relative to the server's default directory.", Destructive = true, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
//    public string DeleteDirectory(string relativePath) 
//        => fileSystemService.DeleteDirectory(relativePath);

//    [McpServerTool(Title = "Move file relative to the server's default directory.", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
//    public string MoveFile(string sourceFullFilePath, string destinationFullFilePath)
//        => fileSystemService.MoveFile(sourceFullFilePath, destinationFullFilePath);

//    [McpServerTool(Title = "Copy file relative to the server's default directory.", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
//    public string CopyFile(string sourceFullFilePath, string destinationFullFilePath) 
//        => fileSystemService.CopyFile(sourceFullFilePath, destinationFullFilePath);

//    [McpServerTool(Title = "Statistic file or directory relative to the server's default directory.", Destructive = false, Idempotent = true, ReadOnly = true, UseStructuredContent = true)]
//    public StatisticResult Statistic(string fullPath) 
//        => fileSystemService.GetStatistic(fullPath);

//    [McpServerTool(Title = "Exists relative to the server's default directory.", Destructive = false, Idempotent = true, ReadOnly = true, UseStructuredContent = true)]
//    public ExistsResult Exists(string fullPath) 
//        => fileSystemService.Exists(fullPath);
//}
