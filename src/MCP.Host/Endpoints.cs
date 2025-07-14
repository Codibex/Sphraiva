using System.Net.Mime;
using MCP.Host.Contracts;
using MCP.Host.Data;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Data;

namespace MCP.Host;

public static class Endpoints
{
    private static readonly ChatHistoryAgentThread _agentThread = new();

    public static void MapEndpoints(this WebApplication app)
    {
        app.MapPost("/chat", (async (ChatRequest request, Kernel kernel, CancellationToken cancellationToken) =>
        {
            var settings = new OllamaPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                Temperature = 0,
            };
            
            var result = await kernel.InvokePromptAsync(request.Message, new KernelArguments(settings), cancellationToken: cancellationToken);

            var value = result.GetValue<string>();

            return Results.Ok(value);
        }));

        app.MapPost("/function-test", (async (Kernel kernel, CancellationToken cancellationToken) =>
        {
            if (kernel.Plugins.TryGetFunction("Sphraiva", "read_file", out var func))
            {
                var result = await func.InvokeAsync(new KernelArguments
                {
                    ["file"] = "Recipe.md"
                }, cancellationToken);
                return Results.Ok(result?.ToString());
            }

            return Results.BadRequest("Function not callable");
        }));

        app.MapPost("/agent", (async (ChatRequest request, Kernel kernel, VectorStoreTextSearch<TextParagraph> textSearchStore, HttpResponse response, CancellationToken cancellationToken) =>
        {
            response.ContentType = MediaTypeNames.Text.EventStream;

            ChatCompletionAgent agent =
                new()
                {
                    Name = "FriendlyAssistant",
                    Instructions = """
                                   You are a friendly assistant.
                                   Use the available tools to answer user requests.
                                   """,
                    Kernel = kernel,
                    Arguments = new KernelArguments(
                        new OllamaPromptExecutionSettings
                        {
                            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                        })
                };
#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0130
            if (_agentThread.AIContextProviders.Providers.Count == 0)
            {
                _agentThread.AIContextProviders.Add(new TextSearchProvider(textSearchStore));
            }
#pragma warning restore SKEXP0130
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            var messages = new List<StreamingChatMessageContent>();
            await agent
                .InvokeStreamingAsync(request.Message, _agentThread, cancellationToken: cancellationToken)
                .AggregateAsync(messages, (current, responseItem) =>
                {
                    current.Add(responseItem.Message);
                    return current;
                }, cancellationToken: cancellationToken);
            
            await foreach (var result in agent.InvokeAsync(request.Message, _agentThread, cancellationToken: cancellationToken))
            {
                var content = result.Message.Content;
                await response.WriteAsync(content ?? "No response", cancellationToken);
                await response.Body.FlushAsync(cancellationToken);
            }
            //return Results.Ok(new
            //{
            //    response.Content,
            //    response.Role
            //});
        }));
    }
}