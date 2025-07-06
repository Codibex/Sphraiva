using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace MCP.Host;

public static class Endpoints
{
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
    }
}

public record ReadFileParameters(string Path);

public record ChatRequest(string Message);