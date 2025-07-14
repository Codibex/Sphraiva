using MCP.BackgroundWorker.FileSystem.Services;
using MCP.BackgroundWorker.FileSystem.Setup;

var builder = Host.CreateApplicationBuilder(args);
var configuration = builder.Configuration;

var ollamaClient = builder.Services.RegisterOllamaClient(configuration);

builder.Services
    .AddSemantikKernel(ollamaClient)
    .AddBackgroundWorkerServices(configuration);

await Task.Delay(5000);

var host = builder.Build();
host.Run();
