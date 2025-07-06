using ModelContextProtocol.Server;

[McpServerToolType]
public static class FileSystemTool
{
    [McpServerTool(Title = "Read file", Destructive = false, Idempotent = true, ReadOnly = true, UseStructuredContent = true)]
    public static Task<ReadFileResult> ReadFile(ReadFileParameters parameters)
    {
        string? path = parameters.Path;
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path is required");

        path = $"../data/{path}";

        var content = File.ReadAllText(path);
        return Task.FromResult(ReadFileResult.SuccessResult(content));
    }

    [McpServerTool(Title = "Write file", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    public static Task<WriteFileResult> WriteFile(WriteFileParameters parameters)
    {
        string? path = parameters.Path;
        string? content = parameters.Content;
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path is required");
        File.WriteAllText(path, content ?? "");
        return Task.FromResult(WriteFileResult.SuccessResult());
    }
}

public record ReadFileParameters(string Path);
public record ReadFileResult(string? Content, string? ErrorMessage = null)
{
    public static ReadFileResult SuccessResult(string content) => new(content);
    public static ReadFileResult Failure(string errorMessage) => new(null, errorMessage);
}

public record WriteFileParameters(string Path, string Content);
public record WriteFileResult(bool Success, string? ErrorMessage = null)
{
    public static WriteFileResult SuccessResult() => new(true);
    public static WriteFileResult Failure(string errorMessage) => new(false, errorMessage);
}