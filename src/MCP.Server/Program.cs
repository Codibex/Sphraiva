using Docker.DotNet;
using MCP.Server.Services;
using MCP.Server.Services.DevContainers;
using MCP.Server.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithStdioServerTransport()
    .WithToolsFromAssembly()
    .WithResourcesFromAssembly()
    .WithPromptsFromAssembly();

builder.Services
    .AddScoped<IFileSystemService, FileSystemService>()
    .AddScoped<IDevContainerService, DevContainerService>()
    .AddScoped<IDevContainerBuilder, DevContainerBuilder>()
    .AddScoped<IDevContainerCreator, DevContainerCreator>()
    .AddScoped<IDockerTarService, DockerTarService>();

builder.Services.AddTransient(_ => new DockerClientConfiguration().CreateClient());

builder.Services
    .AddOptions<DevContainerSettings>()
    .Bind(builder.Configuration.GetSection(nameof(DevContainerSettings)))
    .ValidateDataAnnotations()
    .ValidateOnStart();

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

app.MapMcp();

app.Run();
