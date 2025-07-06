﻿namespace MCP.Host;

public static class Endpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        // Map the OpenAPI endpoint
        app.MapOpenApi();
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
    }
}
