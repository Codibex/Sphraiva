using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.Ollama;
using OllamaSharp.Models.Chat;

namespace MCP.Host;

public static class Endpoints
{
    private static ChatHistoryAgentThread _agentThread = new ChatHistoryAgentThread();

    public static void MapEndpoints(this WebApplication app)
    {


        app.MapPost("/chat", (async (ChatRequest request, Kernel kernel) =>
        {
            OllamaPromptExecutionSettings settings = new OllamaPromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                Temperature = 0
            };
            //if (kernel.Plugins.TryGetFunction("Sphraiva", "read_file", out var func))
            //{
            //    var foo = await func.InvokeAsync(new KernelArguments()
            //    {
            //        ["parameters"] = new ReadFileParameters("Recipe.md")
            //    });
            //}

            var result = await kernel.InvokePromptAsync(request.Message, new KernelArguments(settings));



            var x = result.GetValue<string>();

            return Results.Ok(x);
        }));

        app.MapPost("/agent", (async (ChatRequest request, Kernel kernel) =>
        {
            ChatCompletionAgent agent =
                new()
                {
                    Name = "FriendlyAssistant",
                    Instructions = "You are a friendly assistant",
                    Kernel = kernel,
                    Arguments = new KernelArguments(new PromptExecutionSettings
                        { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
                };

            ChatMessageContent response = await agent.InvokeAsync(request.Message, _agentThread).FirstAsync();

            return Results.Ok(response.Content);

            //return Results.Ok(new
            //{
            //    response.Content,
            //    response.Role
            //});
        }));
    }
}

public record ReadFileParameters(string Path);

public record ChatRequest(string Message);