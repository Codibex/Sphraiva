using MCP.Host;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(services => services
    .AddConsole()
    .SetMinimumLevel(LogLevel.Trace)
);

builder.Services.AddSemanticKernel(builder.Configuration);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.MapEndpoints();
app.Run();
