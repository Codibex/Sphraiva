using ModelContextProtocol.Server;

namespace MCP.Server;

[McpServerToolType]
public static class FileSystemTool
{
    [McpServerTool(Title = "Read file", Destructive = false, Idempotent = true, ReadOnly = true, UseStructuredContent = true)]
    public static Task<ReadFileResult> ReadFile(string file)
    {
        if (string.IsNullOrWhiteSpace(file))
            throw new ArgumentException("Path is required");

        var path = $"../data/{file}";

        var content = File.ReadAllText(path);
        var result = ReadFileResult.SuccessResult(content);
        return Task.FromResult(result);
    }

    [McpServerTool(Title = "Write file", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    public static Task<WriteFileResult> WriteFile(string file, string content)
    {
        File.WriteAllText(file, content);
        return Task.FromResult(WriteFileResult.SuccessResult());
    }
}

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