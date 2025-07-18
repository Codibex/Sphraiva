using MCP.BackgroundWorker.FileSystem.Contracts;
using MCP.Host.Chat;
using MCP.Host.Contracts;
using MCP.Host.Services;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Data;
using System.Net.Mime;

namespace MCP.Host.Api;

public static class Endpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        app.MapPost("/chat",
            async (ChatRequest request, IKernelProvider kernelProvider, CancellationToken cancellationToken) =>
            {
                var kernel = kernelProvider.Get();

                var settings = new OllamaPromptExecutionSettings
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                    Temperature = 0,
                };
                var result = await kernel.InvokePromptAsync(request.Message, new KernelArguments(settings),
                    cancellationToken: cancellationToken);

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

        app.MapPost("/agent/chat", async (HeaderValueProvider headerValueProvider, ChatRequest request,
                IKernelProvider kernelProvider, VectorStoreTextSearch<TextParagraph> textSearchStore,
                ChatCache chatCache, HttpResponse response, CancellationToken cancellationToken) =>
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
                                       .InvokeStreamingAsync(request.Message, thread,
                                           cancellationToken: cancellationToken))
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

        app.MapPost("/agent/code", async (HeaderValueProvider headerValueProvider, ChatRequest request,
                IKernelProvider kernelProvider, ChatCache chatCache, HttpResponse response,
                CancellationToken cancellationToken) =>
            {
                var chatId = headerValueProvider.ChatId!.Value;
                var thread = chatCache.GetOrCreateThread(chatId);

                var kernel = kernelProvider.Get();

                ChatCompletionAgent agent =
                    new()
                    {
                        Name = "CodingAgent",
                        Instructions = """
                                       You are an autonomous coding agent. You are an expert in implementing the requested changes independently, step by step.
                                       You are a focused coding agent and do not engage in discussions unrelated to implementation or requirements. 
                                       If users ask unrelated questions, politely inform them that you only address topics relevant to the requested changes.
                                       Ask the user if required information is missing.

                                       ## User requirements

                                       The user has to provide the following data:
                                       1. Instruction name for the docker container creation. If not initially provided, ask the user for it.
                                       2. A repository name. If not initially provided, ask the user for it.
                                       3. A set of requirements for changes.

                                       A pull request creation is not necessary. The user creates the pull request.

                                       ## Necessary steps you have to do

                                       1. Create a development container for the task.
                                       2. Clone the specified repository.
                                       3. Create a new branch for the implementation. Use a name that matches the requirement in short.
                                       4. Analyze the current state of the repository and the user's requirements, and identify the necessary changes.
                                       5. Create a change plan.
                                       6. Communicate the change plan to the user and wait for feedback or clarification before proceeding.
                                       7. If the user provides feedback or additional changes, update the change plan accordingly. Once everything is confirmed and clear, start implementing the changes step by step.
                                       8. Build the solution to check if the made changes are correct. Otherwise fix the build issues.
                                       9. Commit the changes with meaningful commit messages.
                                       10. Push the branch to the remote repository.
                                       11. After completion, clean up and remove the development container.
                                       """,
                        Kernel = kernel,
                        Arguments = new KernelArguments(
                            new OllamaPromptExecutionSettings
                            {
                                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                            })
                    };

                try
                {
                    await foreach (var result in agent
                                       .InvokeStreamingAsync(request.Message, thread,
                                           cancellationToken: cancellationToken))
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
