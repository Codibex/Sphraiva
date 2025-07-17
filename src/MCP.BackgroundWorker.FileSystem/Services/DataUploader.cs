using MCP.BackgroundWorker.FileSystem.Contracts;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace MCP.BackgroundWorker.FileSystem.Services;

internal class DataUploader(VectorStore vectorStore, IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
{
    private enum ActionType
    {
        None,
        Insert,
        Update,
        Delete
    }

    public async Task GenerateEmbeddingsAndUpload(string collectionName, IEnumerable<TextParagraph> textParagraphs)
    {
        var collection = vectorStore.GetCollection<Guid, TextParagraph>(collectionName);
        await collection.EnsureCollectionExistsAsync();

        foreach (var paragraph in textParagraphs)
        {
            var actionType = ActionType.Insert;
            await foreach (var foundParagraph in collection.GetAsync(p => p.DocumentUri == paragraph.DocumentUri && p.ParagraphId == paragraph.ParagraphId, 1))
            {
                if(!foundParagraph.Text.Equals(paragraph.Text))
                {
                    actionType = ActionType.Update;
                    paragraph.TextEmbedding = foundParagraph.TextEmbedding;
                    break;
                }
                actionType = ActionType.None;
                break;
            }

            if (actionType == ActionType.None)
            {
                continue;
            }

            var embedding = await embeddingGenerator.GenerateAsync(paragraph.Text);
            paragraph.TextEmbedding = embedding.Vector;
            await collection.UpsertAsync(paragraph);
        }
    }
}
