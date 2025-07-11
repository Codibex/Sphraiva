namespace MCP.Server.Settings;

public class DockerImage
{
    /// <summary>
    /// The instruction keyword used to request creation of this Docker image via the LLM.
    /// This value is sent to the LLM (Large Language Model) to select the appropriate image configuration,
    /// for example, "net9" to create a .NET 9 development container.
    /// </summary>
    public string InstructionName { get; set; } = null!;
    public string ImageName { get; set; } = null!;
    public string Path { get; set; } = null!;
    public ICollection<string>? VolumeBinds { get; set; }
}