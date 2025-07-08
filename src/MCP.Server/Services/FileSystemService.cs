using MCP.Server.Common;

namespace MCP.Server.Services;

public class FileSystemService : IFileSystemService
{
    private readonly string _dataDirectory = "../data/";

    public async Task<OperationResult<string>> ReadFileAsync(string relativePath)
    {
        var path = Path.Combine(_dataDirectory, relativePath);
        if (!File.Exists(path))
        {
            return OperationResult<string>.Failure("File not found!");
        }
        var content = await File.ReadAllTextAsync(path);
        return OperationResult<string>.Success(content!);
    }

    public async Task<OperationResult> WriteFileAsync(string relativePath, string content)
    {
        var path = Path.Combine(_dataDirectory, relativePath);
        await File.WriteAllTextAsync(path, content);

        return OperationResult.Success();
    }
}