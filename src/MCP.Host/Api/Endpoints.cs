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

            try
            {
                await foreach (var result in agent
                                   .InvokeStreamingAsync(request.Message, thread, cancellationToken: cancellationToken))
                {
                    var content = result.Message.Content ?? string.Empty;

                    await response.WriteAsync(content, cancellationToken);
                    await response.Body.FlushAsync(cancellationToken);
                }
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
            var chatId = headerValueProvider.ChatId!.Value;
            var thread = chatCache.GetOrCreateThread(chatId);

            var kernel = kernelProvider.Get();

            ChatCompletionAgent agent =
                new()
                {
                    Name = "CodingAgent",
                    Instructions = """
                                   You are an autonomous coding agent. Always consider the previous chat history and continue the workflow from the last completed step. 
                                   If you have already collected information, proceed to the next logical step in the process. 
                                   Only ask the user for missing information if required, and wait for their response before continuing. 
                                   After each step, send a status update and wait for user confirmation before proceeding. 
                                   If an error occurs, report it and wait for further instructions.
                                   
                                   When the user describes a task, proceed step by step as follows:
                                   1. Check if you have all needed information for the next steps. 
                                      If you need information for the next steps, ask the user and wait for a response before continuing.
                                   2. Create a development container for the task. 
                                      Use the instruction name provided by the user to determine the container image.
                                   3. Clone the specified repository.
                                   4. Create a new branch for the implementation.
                                   5. Analyze the current state of the repository and the user's requirement. 
                                      Based on both, create a detailed plan for the required changes. 
                                      Communicate this plan to the user and wait for feedback or clarification before proceeding.
                                   6. Apply the changes step by step. After each change, log the action, affected files, and provide a git diff as a status update.
                                   7. Commit the changes with meaningful commit messages.
                                   8. Push the branch to the remote repository.
                                   9. After completion, clean up and remove the development container.
                                   10. After each step, send a status update to the user. If an error occurs, report it and wait for further instructions.
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

            try
            {
                await foreach (var result in agent
                                   .InvokeStreamingAsync(request.Message, thread, cancellationToken: cancellationToken))
                {
                    var content = result.Message.Content ?? string.Empty;

                    await response.WriteAsync(content, cancellationToken);
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