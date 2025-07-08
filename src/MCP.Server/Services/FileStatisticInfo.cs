namespace MCP.Server.Services;

public record FileStatisticInfo
{
    public bool Exists { get; set; }
    public bool IsDirectory { get; set; }
    public long? Size { get; set; }
    public DateTime? LastModified { get; set; }
    public DateTime? Created { get; set; }
}