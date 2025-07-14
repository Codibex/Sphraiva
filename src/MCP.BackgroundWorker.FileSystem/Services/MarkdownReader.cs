namespace MCP.BackgroundWorker.FileSystem.Services;

internal class MarkdownReader
{
    public static List<TextParagraph> ReadMarkdown(Stream documentContents, string documentUri)
    {
        using StreamReader reader = new(documentContents);
        var content = reader.ReadToEnd();

        var paragraphs = content.Split(["\r\n\r\n", "\n\n"], StringSplitOptions.RemoveEmptyEntries);
        var result = new List<TextParagraph>();
        int paragraphIndex = 0;
        foreach (var paragraph in paragraphs)
        {
            var trimmed = paragraph.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                continue;
            }
            result.Add(new TextParagraph
            {
                Text = trimmed,
                DocumentUri = documentUri,
                Key = Guid.NewGuid(),
                ParagraphId = paragraphIndex
            });
            paragraphIndex++;
        }
        return result;
    }
}
