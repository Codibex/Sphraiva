using MCP.Server.Results;

namespace MCP.Server.Services.FileSystem;

public class FileSystemService : IFileSystemService
{
    private readonly string _dataDirectory = "../data/";

    public async Task<ReadFileResult> ReadFileAsync(string fullFilePath)
    {
        var path = Path.Combine(_dataDirectory, fullFilePath);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"File not found: {fullFilePath}");
        }
        var content = await File.ReadAllTextAsync(path);
        return new ReadFileResult(Path.GetFileName(fullFilePath), content);
    }

    public async Task<string> WriteFileAsync(string fullFilePath, string content)
    {
        var path = Path.Combine(_dataDirectory, fullFilePath);

        if (File.Exists(path))
        {
            throw new InvalidOperationException("File already exists. Use a different name or delete the existing file first.");
        }

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        await File.WriteAllTextAsync(path, content);
        return "File written successfully: " + fullFilePath;
    }

    public string DeleteFile(string fullFilePath)
    {
        var path = Path.Combine(_dataDirectory, fullFilePath);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"File not found: {fullFilePath}");
        }
        File.Delete(path);
        return $"File deleted successfully: {fullFilePath}";
    }

    public ListDirectoryResult ListDirectory(string relativePath)
    {
        var path = Path.Combine(_dataDirectory, relativePath);
        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"Directory not found: {relativePath}");
        }

        var files = Directory.GetFiles(path);
        var directories = Directory.GetDirectories(path);
        return new ListDirectoryResult(directories, files);
    }

    public string CreateDirectory(string relativePath)
    {
        var path = Path.Combine(_dataDirectory, relativePath);
        if (Directory.Exists(path))
        {
            throw new Exception($"Directory already exists: {relativePath}");
        }
        Directory.CreateDirectory(path);
        return $"Directory created successfully: {relativePath}";
    }

    public string DeleteDirectory(string relativePath)
    {
        var path = Path.Combine(_dataDirectory, relativePath);
        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"Directory not found: {relativePath}");
        }
        Directory.Delete(path, true);
        return $"Directory deleted successfully: {relativePath}";
    }

    public string MoveFile(string sourceFullFilePath, string destinationFullFilePath)
    {
        var sourcePath = Path.Combine(_dataDirectory, sourceFullFilePath);
        var destPath = Path.Combine(_dataDirectory, destinationFullFilePath);
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException($"File not found: {sourceFullFilePath}");
        }
        File.Move(sourcePath, destPath, true);
        return $"File moved successfully: {destinationFullFilePath}";
    }

    public string CopyFile(string sourceFullFilePath, string destinationFullFilePath)
    {
        var sourcePath = Path.Combine(_dataDirectory, sourceFullFilePath);
        var destPath = Path.Combine(_dataDirectory, destinationFullFilePath);
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException($"File not found: {sourceFullFilePath}");
        }
        File.Copy(sourcePath, destPath, true);
        return $"File copied successfully: {destinationFullFilePath}";
    }

    public StatisticResult GetStatistic(string fullPath)
    {
        var path = Path.Combine(_dataDirectory, fullPath);
        if (File.Exists(path))
        {
            var info = new FileInfo(path);

            return new StatisticResult(
                true,
                "File",
                info.Length,
                info.CreationTimeUtc,
                info.LastWriteTimeUtc
            );
        }

        if (Directory.Exists(path))
        {
            var info = new DirectoryInfo(path);
            return new StatisticResult(
                true,
                "Directory",
                null,
                info.CreationTimeUtc,
                info.LastWriteTimeUtc
            );
        }

        return new StatisticResult(
            false,
            "Unknown",
            null,
            DateTime.MinValue,
            DateTime.MinValue
        );
    }

    public ExistsResult Exists(string fullPath)
    {
        var path = Path.Combine(_dataDirectory, fullPath);
        var fileExists = File.Exists(path);
        if (fileExists)
        {
            return new ExistsResult(true, "File");
        }

        if (Directory.Exists(path))
        {
            return new ExistsResult(true, "Directory");
        }

        return new ExistsResult(false, "Unknown"); 
    }
}