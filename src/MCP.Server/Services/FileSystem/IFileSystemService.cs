using MCP.Server.Results;

namespace MCP.Server.Services.FileSystem;

public interface IFileSystemService
{
    Task<ReadFileResult> ReadFileAsync(string fullFilePath);
    Task<string> WriteFileAsync(string fullFilePath, string content);
    string DeleteFile(string fullFilePath);
    ListDirectoryResult ListDirectory(string relativePath);
    string CreateDirectory(string relativePath);
    string DeleteDirectory(string relativePath);
    string MoveFile(string sourceFullFilePath, string destinationFullFilePath);
    string CopyFile(string sourceFullFilePath, string destinationFullFilePath);
    StatisticResult GetStatistic(string fullPath);
    ExistsResult Exists(string fullPath);
}