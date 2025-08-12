namespace MCP.Host.Chat;

/// <summary>
/// Provider for header values, e.g. ChatId. Provided via DI and populated in middleware.
/// </summary>
public class HeaderValueProvider
{
    /// <summary>
    /// The unique chat identifier.
    /// </summary>
    public Guid? ChatId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the connection to the coding agent hub.
    /// </summary>
    public string? CodingAgentHubConnectionId { get; set; }
}
