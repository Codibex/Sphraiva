
namespace MCP.BackgroundWorker.FileSystem;

internal class MarkdownReader
{
    public static TextParagraph ReadMarkdown(Stream documentContents, string documentUri)
    {
        // Read the markdown file from the stream.
        using StreamReader reader = new(documentContents);
        var content = reader.ReadToEnd();

        return new TextParagraph
        {
            Text = content,
            DocumentUri = documentUri,
            Key = Guid.NewGuid(),
            ParagraphId = Guid.NewGuid().ToString(),
        };
    }
}
