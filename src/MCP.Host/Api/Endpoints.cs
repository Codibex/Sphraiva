using System.Net.Mime;
using MCP.BackgroundWorker.FileSystem.Contracts;
using MCP.Host.Contracts;
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

        app.MapPost("/agent/chat", async (HeaderValueProvider headerValueProvider, ChatRequest request, IKernelProvider kernelProvider, VectorStoreTextSearch<TextParagraph> textSearchStore, ChatCache chatCache, HttpResponse response, CancellationToken cancellationToken) =>
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

        app.MapDelete("/agent/chat", (HeaderValueProvider headerValueProvider, ChatCache chatCache) =>
            {
                chatCache.Remove(headerValueProvider.ChatId!.Value);
                return Results.NoContent();
            })
        .AddEndpointFilter<RequireChatIdEndpointFilter>();

        app.MapPost("/agent/code", async (HeaderValueProvider headerValueProvider, ChatRequest request, IKernelProvider kernelProvider, VectorStoreTextSearch<TextParagraph> textSearchStore, ChatCache chatCache, HttpResponse response, CancellationToken cancellationToken) =>
        {
            response.ContentType = MediaTypeNames.Text.EventStream;

            var chatId = headerValueProvider.ChatId!.Value;
            var thread = chatCache.GetOrCreateThread(chatId);

            var kernel = kernelProvider.Get();

            ChatCompletionAgent agent =
                new()
                {
                    Name = "CodingAgent",
                    Instructions = """
                        You are an autonomous coding agent. When the user describes a task, you must independently:
                        1. Create a development container for the task.
                        2. Clone the specified repository.
                        3. Create a new branch for the implementation.
                        4. Plan the required changes and communicate your plan. If you need more information, ask the user for clarification before proceeding with the implementation.
                        5. Apply the changes step by step.
                        6. For each change, log the action, affected files, and provide a git diff as a status update.
                        7. Commit the changes with meaningful commit messages.
                        8. Push the branch to the remote repository.
                        9. After completion, clean up and remove the development container.
                        10. Send status updates and error messages to the user throughout the process.
                        Do not create a pull request. Always use markdown for code and diffs in your status updates.
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
            }
            catch (TaskCanceledException)
            {
                // Optionally log or ignore; do not treat as error
            }
        })
        .AddEndpointFilter<RequireChatIdEndpointFilter>();
    }
}