using System.Net.Mime;
using MCP.Host.Contracts;
using MCP.Host.Data;
using MCP.Host.Services;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Data;
using MCP.Host.Chat;

namespace MCP.Host.Api;

public static class Endpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        app.MapPost("/chat", async (ChatRequest request, IKernelProvider kernelProvider, CancellationToken cancellationToken) =>
        {
            var kernel = kernelProvider.Get();

            var settings = new OllamaPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                Temperature = 0,
            };
            var result = await kernel.InvokePromptAsync(request.Message, new KernelArguments(settings), cancellationToken: cancellationToken);

            var value = result.GetValue<string>();

            return Results.Ok(value);
        });

        app.MapPost("/function-test", async (IKernelProvider kernelProvider, CancellationToken cancellationToken) =>
        {
            var kernel = kernelProvider.Get();
            if (kernel.Plugins.TryGetFunction("Sphraiva", "read_file", out var func))
            {
                var result = await func.InvokeAsync(new KernelArguments
                {
                    ["file"] = "Recipe.md"
                }, cancellationToken);
                return Results.Ok(result?.ToString());
            }

            return Results.BadRequest("Function not callable");
        });

        app.MapPost("/agent", async (HeaderValueProvider headerValueProvider, ChatRequest request, IKernelProvider kernelProvider, VectorStoreTextSearch<TextParagraph> textSearchStore, ChatCache chatCache, HttpResponse response, CancellationToken cancellationToken) =>
        {
            response.ContentType = MediaTypeNames.Text.EventStream;

            var chatId = headerValueProvider.ChatId!.Value;
            var thread = chatCache.GetOrCreateThread(chatId);

            var kernel = kernelProvider.Get();

            ChatCompletionAgent agent =
                new()
                {
                    Name = "FriendlyAssistant",
                    Instructions = """
                                   You are a helpful and friendly assistant.
                                   Feel free to have a conversational and engaging tone.
                                   Answer questions informatively, but you can also ask clarifying questions or make small talk if appropriate.
                                   Use simple markdown to format your responses.
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
            if (thread.AIContextProviders.Providers.Count == 0)
            {
                thread.AIContextProviders.Add(new TextSearchProvider(textSearchStore));
            }
#pragma warning restore SKEXP0130
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            var messages = new List<StreamingChatMessageContent>();

            try
            {
                await agent
                     .InvokeStreamingAsync(request.Message, thread, cancellationToken: cancellationToken)
                     .AggregateAsync(messages, (current, responseItem) =>
                     {
                         current.Add(responseItem.Message);
                         return current;
                     }, cancellationToken: cancellationToken);

                await foreach (var result in agent.InvokeAsync(request.Message, thread, cancellationToken: cancellationToken))
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
            }
            catch (TaskCanceledException)
            {
                // Optionally log or ignore; do not treat as error
            }
        })
        .AddEndpointFilter<RequireChatIdEndpointFilter>();
    }
}