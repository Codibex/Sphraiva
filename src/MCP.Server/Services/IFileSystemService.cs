using MCP.Server.Results;

namespace MCP.Server.Services;

public interface IFileSystemService
{
    Task<ReadFileResult> ReadFileAsync(string relativePath);
    Task<string> WriteFileAsync(string relativePath, string content);
    string DeleteFile(string relativePath);
    ListDirectoryResult ListDirectory(string relativePath);
    string CreateDirectory(string relativePath);
    string DeleteDirectory(string relativePath);
    string MoveFile(string sourceRelativePath, string destRelativePath);
    string CopyFile(string sourceRelativePath, string destRelativePath);
    StatisticResult GetStatistic(string relativePath);
    ExistsResult Exists(string relativePath);
}