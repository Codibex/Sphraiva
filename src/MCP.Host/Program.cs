using MCP.Host.Agents;
using MCP.Host.Api;
using MCP.Host.Chat;
using MCP.Host.Hubs;
using MCP.Host.Plugins;
using MCP.Host.Services;
using MCP.Host.Setup;
using Microsoft.AspNetCore.ResponseCompression;
using System.Net.Mime;

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

builder.Services.AddSignalR();
builder.Services.AddResponseCompression(options =>
{
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat([MediaTypeNames.Application.Octet]);
});

builder.Services.AddTransient<CodingAgentProcess>();
builder.Services.AddTransient<CodingAgentWorkflow>();
builder.Services.AddHostedService<CodingAgentBackgroundService>();
builder.Services.AddSingleton<ICodingAgentChannel, CodingAgentChannel>();
builder.Services.AddSingleton<ICodingAgentWorkflowStore, CodingAgentWorkflowStore>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseResponseCompression();

app.UseMiddleware<HeaderValueProviderMiddleware>();

//app.UseHttpsRedirection();

app.MapHub<CodingAgentHub>("/codeAgentHub");

app.MapEndpoints();
app.MapCodingAgentEndpoints();
app.Run();
