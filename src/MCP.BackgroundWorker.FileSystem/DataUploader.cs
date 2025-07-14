using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace MCP.BackgroundWorker.FileSystem;

internal class DataUploader(VectorStore vectorStore, IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
{
    public async Task GenerateEmbeddingsAndUpload(string collectionName, IEnumerable<TextParagraph> textParagraphs)
    {
        var collection = vectorStore.GetCollection<Guid, TextParagraph>(collectionName);
        await collection.EnsureCollectionExistsAsync();

        foreach (var paragraph in textParagraphs)
        {
            var found = false;
            await foreach (var _ in collection.GetAsync(p => p.DocumentUri == paragraph.DocumentUri, 1))
            {
                found = true;
                break;
            }

            if (found)
            {
                continue;
            }

            var embedding = await embeddingGenerator.GenerateAsync(paragraph.Text);
            paragraph.TextEmbedding = embedding.Vector;
            await collection.UpsertAsync(paragraph);
        }
    }
}