using Microsoft.Extensions.VectorData;

namespace MCP.BackgroundWorker.FileSystem;

/// <summary>
/// Represents a paragraph of text extracted from a document, including its metadata and text embedding.
/// </summary>
/// <remarks>
/// This class is used to store and manage text paragraphs along with their associated metadata and
/// embeddings. It is typically used in scenarios where text needs to be processed or analyzed, such as in vector stores
/// for machine learning applications.
/// </remarks>
internal class TextParagraph
{
    /// <summary>
    /// A unique key for the text paragraph.
    /// </summary>
    [VectorStoreKey]
    public required Guid Key { get; init; }

    /// <summary>
    /// An uri that points at the original location of the document containing the text.
    /// </summary>
    [VectorStoreData]
    public required string DocumentUri { get; init; }

    /// <summary>
    /// The id of the paragraph from the document containing the text.
    /// </summary>
    [VectorStoreData]
    public required string ParagraphId { get; init; }

    /// <summary>
    /// The text of the paragraph.
    /// </summary>
    [VectorStoreData]
    public required string Text { get; init; }

    /// <summary>
    /// The embedding generated from the Text.
    /// </summary>
    [VectorStoreVector(5120)]
    public ReadOnlyMemory<float> TextEmbedding { get; set; }
}
