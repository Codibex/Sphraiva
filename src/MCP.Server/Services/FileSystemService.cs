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
        return OperationResult<string>.Success(content);
    }

    public async Task<OperationResult> WriteFileAsync(string relativePath, string content)
    {
        var path = Path.Combine(_dataDirectory, relativePath);
        await File.WriteAllTextAsync(path, content);
        return OperationResult.Success();
    }

    public Task<OperationResult> DeleteFileAsync(string relativePath)
    {
        var path = Path.Combine(_dataDirectory, relativePath);
        if (!File.Exists(path))
        {
            return Task.FromResult(OperationResult.Failure("File not found!"));
        }
        File.Delete(path);
        return Task.FromResult(OperationResult.Success());
    }

    public Task<OperationResult<DirectoryListInfo>> ListDirectoryAsync(string relativePath)
    {
        var path = Path.Combine(_dataDirectory, relativePath);
        if (!Directory.Exists(path))
        {
            return Task.FromResult(OperationResult<DirectoryListInfo>.Failure("Directory not found!"));
        }
        var files = Directory.GetFiles(path);
        var directories = Directory.GetDirectories(path);
        var info = new DirectoryListInfo(directories, files);
        return Task.FromResult(OperationResult<DirectoryListInfo>.Success(info));
    }

    public Task<OperationResult> CreateDirectoryAsync(string relativePath)
    {
        var path = Path.Combine(_dataDirectory, relativePath);
        if (Directory.Exists(path))
        {
            return Task.FromResult(OperationResult.Failure("Directory already exists!"));
        }
        Directory.CreateDirectory(path);
        return Task.FromResult(OperationResult.Success());
    }

    public Task<OperationResult> DeleteDirectoryAsync(string relativePath)
    {
        var path = Path.Combine(_dataDirectory, relativePath);
        if (!Directory.Exists(path))
        {
            return Task.FromResult(OperationResult.Failure("Directory not found!"));
        }
        Directory.Delete(path, true);
        return Task.FromResult(OperationResult.Success());
    }

    public Task<OperationResult> MoveFileAsync(string sourceRelativePath, string destRelativePath)
    {
        var sourcePath = Path.Combine(_dataDirectory, sourceRelativePath);
        var destPath = Path.Combine(_dataDirectory, destRelativePath);
        if (!File.Exists(sourcePath))
        {
            return Task.FromResult(OperationResult.Failure("Source file not found!"));
        }
        File.Move(sourcePath, destPath, true);
        return Task.FromResult(OperationResult.Success());
    }

    public Task<OperationResult> CopyFileAsync(string sourceRelativePath, string destRelativePath)
    {
        var sourcePath = Path.Combine(_dataDirectory, sourceRelativePath);
        var destPath = Path.Combine(_dataDirectory, destRelativePath);
        if (!File.Exists(sourcePath))
        {
            return Task.FromResult(OperationResult.Failure("Source file not found!"));
        }
        File.Copy(sourcePath, destPath, true);
        return Task.FromResult(OperationResult.Success());
    }

    public Task<OperationResult<FileStatisticInfo>> GetStatisticAsync(string relativePath)
    {
        var path = Path.Combine(_dataDirectory, relativePath);
        if (File.Exists(path))
        {
            var info = new FileInfo(path);
            return Task.FromResult(OperationResult<FileStatisticInfo>.Success(new FileStatisticInfo
            {
                Exists = true,
                IsDirectory = false,
                Size = info.Length,
                LastModified = info.LastWriteTimeUtc,
                Created = info.CreationTimeUtc
            }));
        }

        if (Directory.Exists(path))
        {
            var info = new DirectoryInfo(path);
            return Task.FromResult(OperationResult<FileStatisticInfo>.Success(new FileStatisticInfo
            {
                Exists = true,
                IsDirectory = true,
                Size = null,
                LastModified = info.LastWriteTimeUtc,
                Created = info.CreationTimeUtc
            }));
        }

        return Task.FromResult(OperationResult<FileStatisticInfo>.Success(new FileStatisticInfo { Exists = false }));
    }

    public Task<OperationResult<bool>> ExistsAsync(string relativePath)
    {
        var path = Path.Combine(_dataDirectory, relativePath);
        bool exists = File.Exists(path) || Directory.Exists(path);
        return Task.FromResult(OperationResult<bool>.Success(exists));
    }
}