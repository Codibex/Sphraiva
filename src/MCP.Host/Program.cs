using MCP.Host.Api;
using MCP.Host.Setup;
using MCP.Host.Plugins;
using MCP.Host.Chat;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddLogging(services => services
        .AddConsole()
        .SetMinimumLevel(LogLevel.Trace)
);

builder.Services
    .AddSemanticKernel(builder.Configuration)
    .AddQdrantServices(builder.Configuration);

builder.Services.AddSingleton<IMcpPluginCache, McpPluginCache>();
builder.Services.AddHostedService<McpPluginCacheBackgroundService>();

builder.Services.AddScoped<HeaderValueProvider>();
builder.Services.AddSingleton<ChatCache>();
builder.Services.AddHostedService<ChatCacheCleanupService>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<HeaderValueProviderMiddleware>();

//app.UseHttpsRedirection();

app.MapEndpoints();
app.MapCodingAgentEndpoints();
app.Run();
