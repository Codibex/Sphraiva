using MCP.Host.Data;
using Microsoft.SemanticKernel.Data;

namespace MCP.Host.Services;

/// <summary>
/// Result mapper which converts a DataModel to a TextSearchResult.
/// </summary>
internal sealed class TextParagraphTextSearchResultMapper : ITextSearchResultMapper
{
    /// <inheritdoc />
    public TextSearchResult MapFromResultToTextSearchResult(object result)
    {
        if (result is TextParagraph dataModel)
        {
            return new TextSearchResult(value: dataModel.Text)
            {
                Name = dataModel.DocumentUri, 
                Link = dataModel.DocumentUri,
            };
        }
        throw new ArgumentException($"Invalid result type. Expected: {typeof(TextParagraph)}, Actual: {result.GetType()}.");
    }
}