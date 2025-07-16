using MCP.Server.Settings;
using MCP.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithStdioServerTransport()
    .WithToolsFromAssembly()
    .WithResourcesFromAssembly()
    .WithPromptsFromAssembly();

builder.Services
    .AddFileSystemServices()
    .AddDevContainerServices()
    .AddGitServices();

builder.Services
    .AddOptions<DevContainerSettings>()
    .Bind(builder.Configuration.GetSection(nameof(DevContainerSettings)))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.MapHealthChecks("/health");
app.MapMcp();

app.Run();
