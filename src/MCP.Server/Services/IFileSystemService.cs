using MCP.Server.Common;

namespace MCP.Server.Services;

public interface IFileSystemService
{
    Task<OperationResult<string>> ReadFileAsync(string relativePath);
    Task<OperationResult> WriteFileAsync(string relativePath, string content);
}