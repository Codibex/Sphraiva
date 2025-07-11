using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace MCP.Host;

public static class Endpoints
{
    private static readonly ChatHistoryAgentThread _agentThread = new();

    public static void MapEndpoints(this WebApplication app)
    {
        app.MapPost("/chat", (async (ChatRequest request, Kernel kernel) =>
        {
            var settings = new OllamaPromptExecutionSettings
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

            var value = result.GetValue<string>();

            return Results.Ok(value);
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