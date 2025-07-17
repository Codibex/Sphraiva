using MCP.BackgroundWorker.FileSystem.Contracts;
using Microsoft.SemanticKernel.Data;

namespace MCP.Host.Services;

/// <summary>
/// String mapper which converts a DataModel to a string.
/// </summary>
internal sealed class TextParagraphTextSearchStringMapper : ITextSearchStringMapper
{
    /// <inheritdoc />
    public string MapFromResultToString(object result)
    {
        if (result is TextParagraph dataModel)
        {
            return dataModel.Text;
        }
        throw new ArgumentException($"Invalid result type. Expected: {typeof(TextParagraph)}, Actual: {result.GetType()}.");
    }
}
