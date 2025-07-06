using ModelContextProtocol.Server;

[McpServerToolType]
public static class FileSystemTool
{
    [McpServerTool]
    public static Task<object?> ReadFile(dynamic parameters)
    {
        string? path = parameters?.path as string;
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path is required");
        var content = File.ReadAllText(path);
        return Task.FromResult<object?>(new { content });
    }

    [McpServerTool]
    public static Task<object?> WriteFile(dynamic parameters)
    {
        string? path = parameters?.path as string;
        string? content = parameters?.content as string;
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path is required");
        File.WriteAllText(path, content ?? "");
        return Task.FromResult<object?>(new { success = true });
    }
}