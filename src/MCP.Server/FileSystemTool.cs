using MCP.Server.Extensions;
using MCP.Server.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MCP.Server;

[McpServerToolType]
[Description(
    """
    Exposes server-side file system operations for agents or tools to read from and write to files in the server's data directory.
    Use these tools to programmatically access or modify file contents as part of automated workflows, agent actions, or user requests.
    All file paths must be relative to the server's data directory for security reasons.
    """
)]
public static class FileSystemTool
{
    [McpServerTool(Title = "Read file", Destructive = false, Idempotent = true, ReadOnly = true, UseStructuredContent = true)]
    [Description(
        """
        Reads the content of a file from the server's file system.
        - 'file': Relative path to the server's data directory (e.g., 'example.txt' resolves to '../data/example.txt').
        Returns the file content as a string if successful, or an error message if the file does not exist or cannot be read.
        Use this tool when you need to retrieve the content of a file for processing, display, or validation.
        Sample phrases:
        - "Can you show me the content of Recipe.md?"
        - "Please read the file Recipe.md and display its contents."
        - "What is inside the file Recipe.md?"
        - "Fetch the contents of Recipe.md."
        - "I need to see what's written in Recipe.md."
        - "Read Recipe.md and provide its content."
        - "Could you give me the text from Recipe.md?"
        - "Show the contents of the file named Recipe.md."
        """
    )]
    public static async Task<ReadFileResult> ReadFile(IMcpServer server, string file)
    {
        if(!server.TryGetService< IFileSystemService>(out var fileSystemService))
        {
            return ReadFileResult.FailureResult("File system service is not available.");
        }

        var result = await fileSystemService.ReadFileAsync(file);
        return result.IsSuccess 
            ? ReadFileResult.SuccessResult(result.Data) 
            : ReadFileResult.FailureResult(result.ErrorMessage!);
    }

    [McpServerTool(Title = "Write file", Destructive = false, Idempotent = false, ReadOnly = false,
        UseStructuredContent = true)]
    [Description(
        """
        Writes the specified content to a file in the server's data directory.
        - 'file': Relative path to the server's data directory (e.g., 'example.txt' resolves to '../data/example.txt').
        - 'content': The text to write to the file.
        Overwrites the file if it exists, or creates a new file if it does not.
        Use this tool to create or update files as part of an agent's workflow or user request.
        Returns a success result or an error message if the operation fails (e.g., due to missing permissions).
        Sample phrases:
        - "Write 'Hello World' to Recipe.md."
        - "Please save the following text in Recipe.md: ... "
        - "Create a file named Recipe.md with this content: ... "
        - "Update Recipe.md with new content."
        - "Overwrite Recipe.md with the provided text."
        - "Store this information in Recipe.md."
        - "Can you write the text to Recipe.md?"
        - "Save the following content into the file Recipe.md."
        """
    )]
    public static async Task<WriteFileResult> WriteFile(IMcpServer server, string file, string content)
    {
        if (!server.TryGetService<IFileSystemService>(out var fileSystemService))
        {
            return WriteFileResult.FailureResult("File system service is not available.");
        }

        var result = await fileSystemService.WriteFileAsync(file, content);
        return result.IsSuccess
            ? WriteFileResult.SuccessResult()
            : WriteFileResult.FailureResult(result.ErrorMessage!);
    }

    [McpServerTool(Title = "Delete file", Destructive = true, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    [Description(
        """
        Deletes a file from the server's data directory.
        - 'file': Relative path to the file to delete (e.g., 'example.txt').
        Returns a success result if the file was deleted, or an error message if the file does not exist or cannot be deleted.
        Sample phrases:
        - "Delete Recipe.md."
        - "Remove the file Recipe.md."
        - "Can you delete Recipe.md?"
        - "Erase Recipe.md from the data directory."
        """
    )]
    public static async Task<DeleteFileResponse> DeleteFile(IMcpServer server, string file)
    {
        if (!server.TryGetService<IFileSystemService>(out var fileSystemService))
        {
            return DeleteFileResponse.FailureResult("File system service is not available.");
        }

        var result = await fileSystemService.DeleteFileAsync(file);
        return result.IsSuccess ? DeleteFileResponse.SuccessResult() : DeleteFileResponse.FailureResult(result.ErrorMessage!);
    }

    [McpServerTool(Title = "List directory", Destructive = false, Idempotent = true, ReadOnly = true, UseStructuredContent = true)]
    [Description(
        """
        Lists all files and subdirectories in a directory within the server's data directory.
        - 'directory': Relative path to the directory (e.g., '').
        Returns a list of file and directory names, or an error message if the directory does not exist.
        Sample phrases:
        - "List all files in the data directory."
        - "Show me the contents of the folder 'docs'."
        - "Which files are in the directory 'images'?"
        - "List everything in 'myfolder'."
        """
    )]
    public static async Task<ListDirectoryResponse> ListDirectory(IMcpServer server, string directory)
    {
        if (!server.TryGetService<IFileSystemService>(out var fileSystemService))
        {
            return ListDirectoryResponse.FailureResult("File system service is not available.");
        }

        var result = await fileSystemService.ListDirectoryAsync(directory);
        return result is { IsSuccess: true, Data: not null }
            ? ListDirectoryResponse.SuccessResult(result.Data.Directories, result.Data.Files)
            : ListDirectoryResponse.FailureResult(result.ErrorMessage!);
    }

    [McpServerTool(Title = "Create directory", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    [Description(
        """
        Creates a new directory in the server's data directory.
        - 'directory': Relative path for the new directory (e.g., 'newfolder').
        Returns a success result if the directory was created, or an error message if it already exists or cannot be created.
        Sample phrases:
        - "Create a folder named 'archive'."
        - "Make a new directory called '2024'."
        - "Add a directory 'backup'."
        """
    )]
    public static async Task<CreateDirectoryResponse> CreateDirectory(IMcpServer server, string directory)
    {
        if (!server.TryGetService<IFileSystemService>(out var fileSystemService))
        {
            return CreateDirectoryResponse.FailureResult("File system service is not available.");
        }

        var result = await fileSystemService.CreateDirectoryAsync(directory);
        return result.IsSuccess ? CreateDirectoryResponse.SuccessResult() : CreateDirectoryResponse.FailureResult(result.ErrorMessage!);
    }

    [McpServerTool(Title = "Delete directory", Destructive = true, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    [Description(
        """
        Deletes a directory and all its contents from the server's data directory.
        - 'directory': Relative path to the directory to delete (e.g., 'oldfolder').
        Returns a success result if the directory was deleted, or an error message if it does not exist or cannot be deleted.
        Sample phrases:
        - "Delete the folder 'temp'."
        - "Remove directory 'backup'."
        - "Erase '2023' directory."
        """
    )]
    public static async Task<DeleteDirectoryResponse> DeleteDirectory(IMcpServer server, string directory)
    {
        if (!server.TryGetService<IFileSystemService>(out var fileSystemService))
        {
            return DeleteDirectoryResponse.FailureResult("File system service is not available.");
        }

        var result = await fileSystemService.DeleteDirectoryAsync(directory);
        return result.IsSuccess ? DeleteDirectoryResponse.SuccessResult() : DeleteDirectoryResponse.FailureResult(result.ErrorMessage!);
    }

    [McpServerTool(Title = "Move file", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    [Description(
        """
        Moves or renames a file within the server's data directory.
        - 'sourceFile': Relative path to the source file.
        - 'destFile': Relative path for the destination file.
        Returns a success result if the file was moved or renamed, or an error message if the source file does not exist or the operation fails.
        Sample phrases:
        - "Move Recipe.md to archive/Recipe.md."
        - "Rename file 'old.txt' to 'new.txt'."
        - "Move 'notes.txt' into the 'docs' folder."
        """
    )]
    public static async Task<MoveFileResponse> MoveFile(IMcpServer server, string sourceFile, string destFile)
    {
        if (!server.TryGetService<IFileSystemService>(out var fileSystemService))
        {
            return MoveFileResponse.FailureResult("File system service is not available.");
        }

        var result = await fileSystemService.MoveFileAsync(sourceFile, destFile);
        return result.IsSuccess ? MoveFileResponse.SuccessResult() : MoveFileResponse.FailureResult(result.ErrorMessage!);
    }

    [McpServerTool(Title = "Copy file", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    [Description(
        """
        Copies a file within the server's data directory.
        - 'sourceFile': Relative path to the source file.
        - 'destFile': Relative path for the destination file.
        Returns a success result if the file was copied, or an error message if the source file does not exist or the operation fails.
        Sample phrases:
        - "Copy Recipe.md to backup/Recipe.md."
        - "Duplicate 'notes.txt' as 'notes_copy.txt'."
        - "Copy 'image.png' into the 'images' folder."
        """
    )]
    public static async Task<CopyFileResponse> CopyFile(IMcpServer server, string sourceFile, string destFile)
    {
        if (!server.TryGetService<IFileSystemService>(out var fileSystemService))
        {
            return CopyFileResponse.FailureResult("File system service is not available.");
        }

        var result = await fileSystemService.CopyFileAsync(sourceFile, destFile);
        return result.IsSuccess ? CopyFileResponse.SuccessResult() : CopyFileResponse.FailureResult(result.ErrorMessage!);
    }

    [McpServerTool(Title = "Statistic file or directory", Destructive = false, Idempotent = true, ReadOnly = true, UseStructuredContent = true)]
    [Description(
        """
        Returns metadata about a file or directory in the server's data directory.
        - 'path': Relative path to the file or directory.
        Returns information such as existence, type (file or directory), size, creation date, and last modified date.
        Sample phrases:
        - "Show me info about Recipe.md."
        - "Get details for the folder 'docs'."
        - "What are the properties of 'image.png'?"
        """
    )]
    public static async Task<StatisticResponse> Statistic(IMcpServer server, string path)
    {
        if (!server.TryGetService<IFileSystemService>(out var fileSystemService))
        {
            return StatisticResponse.FailureResult("File system service is not available.");
        }

        var result = await fileSystemService.GetStatisticAsync(path);
        return result is { IsSuccess: true, Data: not null }
            ? StatisticResponse.SuccessResult(result.Data)
            : StatisticResponse.FailureResult(result.ErrorMessage!);
    }

    [McpServerTool(Title = "Exists", Destructive = false, Idempotent = true, ReadOnly = true, UseStructuredContent = true)]
    [Description(
        """
        Checks if a file or directory exists in the server's data directory.
        - 'path': Relative path to the file or directory.
        Returns true if the file or directory exists, otherwise false.
        Sample phrases:
        - "Does Recipe.md exist?"
        - "Is there a folder named 'archive'?"
        - "Check if 'notes.txt' is present."
        """
    )]
    public static async Task<ExistsResponse> Exists(IMcpServer server, string path)
    {
        if (!server.TryGetService<IFileSystemService>(out var fileSystemService))
        {
            return ExistsResponse.FailureResult("File system service is not available.");
        }

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