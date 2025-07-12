using Microsoft.Extensions.VectorData;

namespace MCP.Host.Data;

public class Document
{
    [VectorStoreKey]
    public ulong DocumentId { get; set; }

    public required string Title { get; set; }

    [VectorStoreData(IsFullTextIndexed = true)]
    public required string Content { get; set; }

    /// <summary>
    /// Filename, URL
    /// </summary>
    public required string Source { get; set; }

    [VectorStoreVector(Dimensions: 1024, DistanceFunction = DistanceFunction.CosineSimilarity, IndexKind = IndexKind.Hnsw)]
    public ReadOnlyMemory<float>? Embedding { get; set; }
}
