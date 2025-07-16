using MCP.Host.Api;
using MCP.Host.Setup;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(services => services
    .AddConsole()
    .SetMinimumLevel(LogLevel.Trace)
);

builder.Services
            .AddSemanticKernel(builder.Configuration)
            .AddQdrantServices(builder.Configuration);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.MapEndpoints();
app.Run();
