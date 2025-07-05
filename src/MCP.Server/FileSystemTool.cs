using ModelContextProtocol.Server;

[McpServerResourceType]
public static class FileSystemTool
{
    [McpServerResource]
    public static Task<object?> ReadFile(dynamic parameters)
    {
        string? path = parameters?.path as string;
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path is required");
        var content = File.ReadAllText(path);
        return Task.FromResult<object?>(new { content });
    }

    [McpServerResource]
    public static Task<object?> WriteFile(dynamic parameters)
    {
        string? path = parameters?.path as string;
        string? content = parameters?.content as string;
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path is required");
        File.WriteAllText(path, content ?? "");
        return Task.FromResult<object?>(new { success = true });
    }

    [McpServerResource]
    public static Task<object?> ListDirectory(dynamic parameters)
    {
        string? path = parameters?.path as string ?? ".";
        var files = Directory.GetFiles(path);
        var dirs = Directory.GetDirectories(path);
        return Task.FromResult<object?>(new { files, dirs });
    }
}
