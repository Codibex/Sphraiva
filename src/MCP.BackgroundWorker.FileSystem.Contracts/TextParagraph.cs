using Microsoft.Extensions.VectorData;

namespace MCP.BackgroundWorker.FileSystem.Contracts;

/// <summary>
/// Represents a paragraph of text extracted from a document, including its metadata and text embedding.
/// </summary>
/// <remarks>
/// This class is used to store and manage text paragraphs along with their associated metadata and
/// embeddings. It is typically used in scenarios where text needs to be processed or analyzed, such as in vector stores
/// for machine learning applications.
/// </remarks>
public class TextParagraph
{
    /// <summary>
    /// A unique key for the text paragraph.
    /// </summary>
    [VectorStoreKey]
    public required Guid Key { get; init; }

    /// <summary>
    /// A URI that points at the original location of the document containing the text.
    /// </summary>
    [VectorStoreData(IsFullTextIndexed = true)]
    public required string DocumentUri { get; init; }

    /// <summary>
    /// The id of the paragraph from the document containing the text.
    /// </summary>
    [VectorStoreData]
    public required int ParagraphId { get; init; }

    /// <summary>
    /// The text of the paragraph.
    /// </summary>
    [VectorStoreData(IsFullTextIndexed = true)]
    public required string Text { get; init; }

    /// <summary>
    /// The embedding generated from the Text.
    /// </summary>
    [VectorStoreVector(5120)]
    public ReadOnlyMemory<float> TextEmbedding { get; set; }
}
