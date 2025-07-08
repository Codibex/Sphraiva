using System.ComponentModel;
using ModelContextProtocol.Server;

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
    public static Task<ReadFileResult> ReadFile(string file)
    {
        if (string.IsNullOrWhiteSpace(file))
            throw new ArgumentException("Path is required");

        var path = $"../data/{file}";

        var content = File.ReadAllText(path);
        var result = ReadFileResult.SuccessResult(content);
        return Task.FromResult(result);
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
    public static Task<WriteFileResult> WriteFile(string file, string content)
    {
        File.WriteAllText(file, content);
        return Task.FromResult(WriteFileResult.SuccessResult());
    }
}

// See best practice here https://modelcontextprotocol.io/docs/concepts/tools#error-handling-2
public record ResultBase<T>(bool Success, T? Result, string? ErrorMessage)
{
};

public record ReadFileResult(bool Success, string? Content, string? ErrorMessage) : ResultBase<string?>(Success, Content, ErrorMessage)
{
    public static ReadFileResult SuccessResult(string? content) => new(true, content, null);
    public static ReadFileResult FailureResult(string errorMessage) => new(false, null, errorMessage);
}

public record WriteFileResult(bool Success, string? ErrorMessage) : ResultBase<string>(Success, null, ErrorMessage)
{
    public static WriteFileResult SuccessResult() => new(true, null);
    public static WriteFileResult FailureResult(string errorMessage) => new(false, errorMessage);
}