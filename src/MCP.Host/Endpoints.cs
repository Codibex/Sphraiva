using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace MCP.Host;

public static class Endpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        // Map the weather forecast endpoint
        app.MapGet("/weatherforecast", () =>
        {
            var summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };
            var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                )).ToArray();
            return forecast;
        }).WithName("GetWeatherForecast");

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