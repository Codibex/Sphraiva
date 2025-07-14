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
            //if (kernel.Plugins.TryGetFunction("Sphraiva", "read_file", out var func))
            //{
            //    var foo = await func.InvokeAsync(new KernelArguments()
            //    {
            //        ["file"] = "Recipe.md"
            //    });
            //    var x = foo?.ToString();
            //}

            var result = await kernel.InvokePromptAsync(request.Message, new KernelArguments(settings), cancellationToken: cancellationToken);

            var value = result.GetValue<string>();

            return Results.Ok(value);
        }));

#pragma warning disable SKEXP0130
        app.MapPost("/agent", (async (ChatRequest request, Kernel kernel, VectorStoreTextSearch<TextParagraph> textSearchStore, CancellationToken cancellationToken) =>
#pragma warning restore SKEXP0130
        {
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
                        new PromptExecutionSettings
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

            var messages = new List<ChatMessageContent>();
            await agent
                .InvokeAsync(request.Message, _agentThread, cancellationToken: cancellationToken)
                .AggregateAsync(messages, (current, response) =>
                {
                    current.Add(response.Message);
                    return current;
                }, cancellationToken: cancellationToken);
            
            var response = string.Join(Environment.NewLine, messages.Select(m => m.Content));
            return Results.Ok(response);
            //return Results.Ok(new
            //{
            //    response.Content,
            //    response.Role
            //});
        }));
    }
}

public record ChatRequest(string Message);