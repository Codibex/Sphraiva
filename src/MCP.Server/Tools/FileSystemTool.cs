using MCP.Server.Results;
using MCP.Server.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MCP.Server.Tools;

[McpServerToolType]
[Description(
    """
    Provides basic file and directory operations in the server's data directory for agents and tools.
    """
)]
public class FileSystemTool(IFileSystemService fileSystemService)
{
    [McpServerTool(Title = "Read file", Destructive = false, Idempotent = true, ReadOnly = true, UseStructuredContent = true)]
    [Description(
        """
        Reads the content of a file from the server's data directory.
        Returns the file content as a string if successful, or an error message if the file does not exist or cannot be read.
        Sample phrases:
        - "Can you show me the content of Recipe.md?"
        - "Fetch the contents of Recipe.md."
        """
    )]
    public async Task<ReadFileResult> ReadFileAsync(
        [Description("Relative path to the server's data directory (e.g., 'example.txt')")]
        string file
    ) => await fileSystemService.ReadFileAsync(file);

    [McpServerTool(Title = "Write file", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    [Description(
        """
        Writes content to a file in the server's data directory. Overwrites if exists, creates if not.
        Returns a success result or an error message if the operation fails.
        Sample phrases:
        - "Write 'Hello World' to Recipe.md."
        - "Create a file named Recipe.md with this content: ... "
        """
    )]
    public async Task<string> WriteFileAsync(
        [Description("Relative path to the server's data directory (e.g., 'example.txt')")]
        string file,
        [Description("The text to write to the file")]
        string content
    ) => await fileSystemService.WriteFileAsync(file, content);

    [McpServerTool(Title = "Delete file", Destructive = true, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    [Description(
        """
        Deletes a file from the server's data directory.
        Returns a success result if deleted, or an error message if not.
        Sample phrases:
        - "Delete Recipe.md."
        - "Erase Recipe.md from the data directory."
        """
    )]
    public string DeleteFile(
        [Description("Relative path to the file to delete (e.g., 'example.txt')")]
        string file
    ) => fileSystemService.DeleteFile(file);

    [McpServerTool(Title = "List directory", Destructive = false, Idempotent = true, ReadOnly = true, UseStructuredContent = true)]
    [Description(
        """
        Lists all files and subdirectories in a directory within the server's data directory.
        Returns a list of file and directory names, or an error message if the directory does not exist.
        Sample phrases:
        - "List all files in the data directory."
        - "Show me the contents of the folder 'docs'."
        """
    )]
    public ListDirectoryResult ListDirectory(
        [Description("Relative path to the directory (e.g., '')")]
        string directory
    ) => fileSystemService.ListDirectory(directory);

    [McpServerTool(Title = "Create directory", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    [Description(
        """
        Creates a new directory in the server's data directory.
        Returns a success result if created, or an error message if not.
        Sample phrases:
        - "Create a folder named 'archive'."
        - "Add a directory 'backup'."
        """
    )]
    public string CreateDirectory(
        [Description("Relative path for the new directory (e.g., 'newfolder')")]
        string directory
    ) => fileSystemService.CreateDirectory(directory);

    [McpServerTool(Title = "Delete directory", Destructive = true, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    [Description(
        """
        Deletes a directory and all its contents from the server's data directory.
        Returns a success result if deleted, or an error message if not.
        Sample phrases:
        - "Delete the folder 'temp'."
        - "Erase '2023' directory."
        """
    )]
    public string DeleteDirectory(
        [Description("Relative path to the directory to delete (e.g., 'oldfolder')")]
        string directory
    ) => fileSystemService.DeleteDirectory(directory);

    [McpServerTool(Title = "Move file", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    [Description(
        """
        Moves or renames a file within the server's data directory.
        Returns a success result if moved/renamed, or an error message if not.
        Sample phrases:
        - "Move Recipe.md to archive/Recipe.md."
        - "Rename file 'old.txt' to 'new.txt'."
        """
    )]
    public string MoveFile(
        [Description("Relative path to the source file")]
        string sourceFile,
        [Description("Relative path for the destination file")]
        string destFile
    ) => fileSystemService.MoveFile(sourceFile, destFile);

    [McpServerTool(Title = "Copy file", Destructive = false, Idempotent = false, ReadOnly = false, UseStructuredContent = true)]
    [Description(
        """
        Copies a file within the server's data directory.
        Returns a success result if copied, or an error message if not.
        Sample phrases:
        - "Copy Recipe.md to backup/Recipe.md."
        - "Duplicate 'notes.txt' as 'notes_copy.txt'."
        """
    )]
    public string CopyFile(
        [Description("Relative path to the source file")]
        string sourceFile,
        [Description("Relative path for the destination file")]
        string destFile
    ) => fileSystemService.CopyFile(sourceFile, destFile);

    [McpServerTool(Title = "Statistic file or directory", Destructive = false, Idempotent = true, ReadOnly = true, UseStructuredContent = true)]
    [Description(
        """
        Returns metadata about a file or directory in the server's data directory.
        Returns information such as existence, type, size, creation date, and last modified date.
        Sample phrases:
        - "Show me info about Recipe.md."
        - "Get details for the folder 'docs'."
        """
    )]
    public StatisticResult Statistic(
        [Description("Relative path to the file or directory")]
        string path
    ) => fileSystemService.GetStatistic(path);

    [McpServerTool(Title = "Exists", Destructive = false, Idempotent = true, ReadOnly = true, UseStructuredContent = true)]
    [Description(
        """
        Checks if a file or directory exists in the server's data directory.
        Returns true if exists, otherwise false.
        Sample phrases:
        - "Does Recipe.md exist?"
        - "Is there a folder named 'archive'?"
        """
    )]
    public ExistsResult Exists(
        [Description("Relative path to the file or directory")]
        string path
    ) => fileSystemService.Exists(path);
}
