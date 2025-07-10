namespace MCP.Server.Settings;

public class DevContainerSettings
{
    public string DataDirectory { get; set; } = null!;
    public string DevContainerImageName { get; set; } = null!;
    public ICollection<string>? VolumeBinds { get; set; } // Host:Container Pfade
}