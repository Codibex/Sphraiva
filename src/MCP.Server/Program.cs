using MCP.Server.Services;
using MCP.Server.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithStdioServerTransport()
    .WithToolsFromAssembly()
    .WithResourcesFromAssembly()
    .WithPromptsFromAssembly();

builder.Services.AddScoped<IFileSystemService, FileSystemService>();
builder.Services.AddScoped<IDevContainerService, DevContainerService>();

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
