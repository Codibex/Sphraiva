using MCP.BackgroundWorker.FileSystem;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

internal class DataUploader(VectorStore vectorStore, IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
{
    /// <summary>
    /// Generate an embedding for each text paragraph and upload it to the specified collection.
    /// </summary>
    /// <param name="collectionName">The name of the collection to upload the text paragraphs to.</param>
    /// <param name="textParagraphs">The text paragraphs to upload.</param>
    /// <returns>An async task.</returns>
    public async Task GenerateEmbeddingsAndUpload(string collectionName, IEnumerable<TextParagraph> textParagraphs)
    {
        var collection = vectorStore.GetCollection<Guid, TextParagraph>(collectionName);
        await collection.EnsureCollectionExistsAsync();

        

        foreach (var paragraph in textParagraphs)
        {
            await foreach (var searchResult in collection.GetAsync(p => p.DocumentUri == paragraph.DocumentUri, 1))
            {
                continue;
            }

            var embedding = await embeddingGenerator.GenerateAsync(paragraph.Text);
            paragraph.TextEmbedding = embedding.Vector;
            await collection.UpsertAsync(paragraph);
        }
    }
}