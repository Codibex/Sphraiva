namespace MCP.Host.Agents.Steps;

public class InputCheckResult
{
    public required string InstructionName { get; set; }
    public required string RepositoryName { get; set; }
    public required string Requirement { get; set; }
    public required ICollection<string> MissingParameters { get; set; } = [];
}