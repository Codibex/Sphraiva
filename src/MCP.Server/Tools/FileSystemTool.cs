using MCP.Server.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MCP.Server.Tools;

[McpServerToolType]
[Description(
    """
    Exposes server-side file system operations for agents or tools to read from and write to files in the server's data directory.
    Use these tools to programmatically access or modify file contents as part of automated workflows, agent actions, or user requests.
    All file paths must be relative to the server's data directory for security reasons.
    """
)]
public class FileSystemTool(IFileSystemService fileSystemService)
{
    [McpServerTool(Title = "Read file", Destructive = false, Idempotent = true, ReadOnly = true, UseStructuredContent = true)]
    [Description(
        """
        Reads the content of a file from the server's data directory.
        Returns the file content as a string if successful, or an error message if the file does not exist or cannot be read.
        Sample phrases:
        - "Can you show me the content of Recipe.md?"
        - "Fetch the contents of Recipe.md."
        """
    )]
    public async Task<ReadFileResult> ReadFile(
        [Description("Relative path to the server's data directory (e.g., 'example.txt')")]
        string file
    )
    {
        var result = await fileSystemService.ReadFileAsync(file);
        return result.IsSuccess 
            ? ReadFileResult.SuccessResult(result.Data) 
            : ReadFileResult.FailureResult(result.ErrorMessage!);
    }

    [McpServerTool(Title = "Write file", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    [Description(
        """
        Writes content to a file in the server's data directory. Overwrites if exists, creates if not.
        Returns a success result or an error message if the operation fails.
        Sample phrases:
        - "Write 'Hello World' to Recipe.md."
        - "Create a file named Recipe.md with this content: ... "
        """
    )]
    public async Task<WriteFileResult> WriteFile(
        [Description("Relative path to the server's data directory (e.g., 'example.txt')")]
        string file,
        [Description("The text to write to the file")]
        string content
    )
    {
        var result = await fileSystemService.WriteFileAsync(file, content);
        return result.IsSuccess
            ? WriteFileResult.SuccessResult()
            : WriteFileResult.FailureResult(result.ErrorMessage!);
    }

    [McpServerTool(Title = "Delete file", Destructive = true, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    [Description(
        """
        Deletes a file from the server's data directory.
        Returns a success result if deleted, or an error message if not.
        Sample phrases:
        - "Delete Recipe.md."
        - "Erase Recipe.md from the data directory."
        """
    )]
    public async Task<DeleteFileResponse> DeleteFile(
        [Description("Relative path to the file to delete (e.g., 'example.txt')")]
        string file
    )
    {
        var result = await fileSystemService.DeleteFileAsync(file);
        return result.IsSuccess ? DeleteFileResponse.SuccessResult() : DeleteFileResponse.FailureResult(result.ErrorMessage!);
    }

    [McpServerTool(Title = "List directory", Destructive = false, Idempotent = true, ReadOnly = true, UseStructuredContent = true)]
    [Description(
        """
        Lists all files and subdirectories in a directory within the server's data directory.
        Returns a list of file and directory names, or an error message if the directory does not exist.
        Sample phrases:
        - "List all files in the data directory."
        - "Show me the contents of the folder 'docs'."
        """
    )]
    public async Task<ListDirectoryResponse> ListDirectory(
        [Description("Relative path to the directory (e.g., '')")]
        string directory
    )
    {
        var result = await fileSystemService.ListDirectoryAsync(directory);
        return result is { IsSuccess: true, Data: not null }
            ? ListDirectoryResponse.SuccessResult(result.Data.Directories, result.Data.Files)
            : ListDirectoryResponse.FailureResult(result.ErrorMessage!);
    }

    [McpServerTool(Title = "Create directory", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    [Description(
        """
        Creates a new directory in the server's data directory.
        Returns a success result if created, or an error message if not.
        Sample phrases:
        - "Create a folder named 'archive'."
        - "Add a directory 'backup'."
        """
    )]
    public async Task<CreateDirectoryResponse> CreateDirectory(
        [Description("Relative path for the new directory (e.g., 'newfolder')")]
        string directory
    )
    {
        var result = await fileSystemService.CreateDirectoryAsync(directory);
        return result.IsSuccess ? CreateDirectoryResponse.SuccessResult() : CreateDirectoryResponse.FailureResult(result.ErrorMessage!);
    }

    [McpServerTool(Title = "Delete directory", Destructive = true, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    [Description(
        """
        Deletes a directory and all its contents from the server's data directory.
        Returns a success result if deleted, or an error message if not.
        Sample phrases:
        - "Delete the folder 'temp'."
        - "Erase '2023' directory."
        """
    )]
    public async Task<DeleteDirectoryResponse> DeleteDirectory(
        [Description("Relative path to the directory to delete (e.g., 'oldfolder')")]
        string directory
    )
    {
        var result = await fileSystemService.DeleteDirectoryAsync(directory);
        return result.IsSuccess ? DeleteDirectoryResponse.SuccessResult() : DeleteDirectoryResponse.FailureResult(result.ErrorMessage!);
    }

    [McpServerTool(Title = "Move file", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    [Description(
        """
        Moves or renames a file within the server's data directory.
        Returns a success result if moved/renamed, or an error message if not.
        Sample phrases:
        - "Move Recipe.md to archive/Recipe.md."
        - "Rename file 'old.txt' to 'new.txt'."
        """
    )]
    public async Task<MoveFileResponse> MoveFile(
        [Description("Relative path to the source file")]
        string sourceFile,
        [Description("Relative path for the destination file")]
        string destFile
    )
    {
        var result = await fileSystemService.MoveFileAsync(sourceFile, destFile);
        return result.IsSuccess ? MoveFileResponse.SuccessResult() : MoveFileResponse.FailureResult(result.ErrorMessage!);
    }

    [McpServerTool(Title = "Copy file", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    [Description(
        """
        Copies a file within the server's data directory.
        Returns a success result if copied, or an error message if not.
        Sample phrases:
        - "Copy Recipe.md to backup/Recipe.md."
        - "Duplicate 'notes.txt' as 'notes_copy.txt'."
        """
    )]
    public async Task<CopyFileResponse> CopyFile(
        [Description("Relative path to the source file")]
        string sourceFile,
        [Description("Relative path for the destination file")]
        string destFile
    )
    {
        var result = await fileSystemService.CopyFileAsync(sourceFile, destFile);
        return result.IsSuccess ? CopyFileResponse.SuccessResult() : CopyFileResponse.FailureResult(result.ErrorMessage!);
    }

    [McpServerTool(Title = "Statistic file or directory", Destructive = false, Idempotent = true, ReadOnly = true, UseStructuredContent = true)]
    [Description(
        """
        Returns metadata about a file or directory in the server's data directory.
        Returns information such as existence, type, size, creation date, and last modified date.
        Sample phrases:
        - "Show me info about Recipe.md."
        - "Get details for the folder 'docs'."
        """
    )]
    public async Task<StatisticResponse> Statistic(
        [Description("Relative path to the file or directory")]
        string path
    )
    {
        var result = await fileSystemService.GetStatisticAsync(path);
        return result is { IsSuccess: true, Data: not null }
            ? StatisticResponse.SuccessResult(result.Data)
            : StatisticResponse.FailureResult(result.ErrorMessage!);
    }

    [McpServerTool(Title = "Exists", Destructive = false, Idempotent = true, ReadOnly = true, UseStructuredContent = true)]
    [Description(
        """
        Checks if a file or directory exists in the server's data directory.
        Returns true if exists, otherwise false.
        Sample phrases:
        - "Does Recipe.md exist?"
        - "Is there a folder named 'archive'?"
        """
    )]
    public async Task<ExistsResponse> Exists(
        [Description("Relative path to the file or directory")]
        string path
    )
    {
        var result = await fileSystemService.ExistsAsync(path);
        return result.IsSuccess ? ExistsResponse.SuccessResult(result.Data) : ExistsResponse.FailureResult(result.ErrorMessage!);
    }
}

// See best practice here https://modelcontextprotocol.io/docs/concepts/tools#error-handling-2
public record ResultBase<T>(bool IsError, T? Result, string? ErrorMessage)
{
};

public record ReadFileResult(bool IsError, string? Content, string? ErrorMessage) : ResultBase<string?>(IsError, Content, ErrorMessage)
{
    public static ReadFileResult SuccessResult(string? content) => new(false, content, null);
    public static ReadFileResult FailureResult(string errorMessage) => new(true, null, errorMessage);
}

public record WriteFileResult(bool IsError, string? ErrorMessage) : ResultBase<string>(IsError, null, ErrorMessage)
{
    public static WriteFileResult SuccessResult() => new(false, null);
    public static WriteFileResult FailureResult(string errorMessage) => new(true, errorMessage);
}

public record DeleteFileResponse(bool IsError, string? ErrorMessage) : ResultBase<string>(IsError, null, ErrorMessage)
{
    public static DeleteFileResponse SuccessResult() => new(false, null);
    public static DeleteFileResponse FailureResult(string errorMessage) => new(true, errorMessage);
}

public record ListDirectoryResponse(bool IsError, IEnumerable<string>? Directories, IEnumerable<string>? Files, string? ErrorMessage) : ResultBase<(IEnumerable<string>? Directories, IEnumerable<string>? Files)?>(IsError, (Directories, Files), ErrorMessage)
{
    public static ListDirectoryResponse SuccessResult(IEnumerable<string>? directories, IEnumerable<string>? files) => new(false, directories, files, null);
    public static ListDirectoryResponse FailureResult(string errorMessage) => new(true, null, null, errorMessage);
}

public record CreateDirectoryResponse(bool IsError, string? ErrorMessage) : ResultBase<string>(IsError, null, ErrorMessage)
{
    public static CreateDirectoryResponse SuccessResult() => new(false, null);
    public static CreateDirectoryResponse FailureResult(string errorMessage) => new(true, errorMessage);
}

public record DeleteDirectoryResponse(bool IsError, string? ErrorMessage) : ResultBase<string>(IsError, null, ErrorMessage)
{
    public static DeleteDirectoryResponse SuccessResult() => new(false, null);
    public static DeleteDirectoryResponse FailureResult(string errorMessage) => new(true, errorMessage);
}

public record MoveFileResponse(bool IsError, string? ErrorMessage) : ResultBase<string>(IsError, null, ErrorMessage)
{
    public static MoveFileResponse SuccessResult() => new(false, null);
    public static MoveFileResponse FailureResult(string errorMessage) => new(true, errorMessage);
}

public record CopyFileResponse(bool IsError, string? ErrorMessage) : ResultBase<string>(IsError, null, ErrorMessage)
{
    public static CopyFileResponse SuccessResult() => new(false, null);
    public static CopyFileResponse FailureResult(string errorMessage) => new(true, errorMessage);
}

public record StatisticResponse(bool IsError, object? Data, string? ErrorMessage) : ResultBase<object?>(IsError, Data, ErrorMessage)
{
    public static StatisticResponse SuccessResult(object? data) => new(false, data, null);
    public static StatisticResponse FailureResult(string errorMessage) => new(true, null, errorMessage);
}

public record ExistsResponse(bool IsError, bool? Exists, string? ErrorMessage) : ResultBase<bool?>(IsError, Exists, ErrorMessage)
{
    public static ExistsResponse SuccessResult(bool? exists) => new(false, exists, null);
    public static ExistsResponse FailureResult(string errorMessage) => new(true, null, errorMessage);
}