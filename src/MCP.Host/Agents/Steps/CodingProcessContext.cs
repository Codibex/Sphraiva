namespace MCP.Host.Agents.Steps;

public class CodingProcessContext
{
    public required string RepositoryName { get; init; }
    public required string Requirement { get; init; }
    public required string ContainerName { get; init; }
    public string? PlannedChanges { get; set; }
}