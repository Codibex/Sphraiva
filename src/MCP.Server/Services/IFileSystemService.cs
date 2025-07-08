using MCP.Server.Common;

namespace MCP.Server.Services;

public interface IFileSystemService
{
    Task<OperationResult<string>> ReadFileAsync(string relativePath);
    Task<OperationResult> WriteFileAsync(string relativePath, string content);
    Task<OperationResult> DeleteFileAsync(string relativePath);
    Task<OperationResult<DirectoryListInfo>> ListDirectoryAsync(string relativePath);
    Task<OperationResult> CreateDirectoryAsync(string relativePath);
    Task<OperationResult> DeleteDirectoryAsync(string relativePath);
    Task<OperationResult> MoveFileAsync(string sourceRelativePath, string destRelativePath);
    Task<OperationResult> CopyFileAsync(string sourceRelativePath, string destRelativePath);
    Task<OperationResult<FileStatisticInfo>> GetStatisticAsync(string relativePath);
    Task<OperationResult<bool>> ExistsAsync(string relativePath);
}