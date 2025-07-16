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
}
