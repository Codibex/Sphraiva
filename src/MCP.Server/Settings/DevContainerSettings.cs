namespace MCP.Server.Settings;

public class DevContainerSettings
{
    public string DataDirectory { get; set; } = null!;
    public string GitUserName { get; set; } = null!;
    public string GitUserEmail { get; set; } = null!;
    public string GithubPatTokenFile { get; set; } = null!;
    public ICollection<DockerImage> DockerImages { get; set; } = null!;

    public DockerImage? GetImageByInstructionName(string instructionName) =>
        DockerImages.FirstOrDefault(image =>
            image.InstructionName.Equals(instructionName, StringComparison.OrdinalIgnoreCase));
}