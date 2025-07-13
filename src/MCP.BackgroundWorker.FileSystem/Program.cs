using MCP.BackgroundWorker.FileSystem;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using OllamaSharp;

var builder = Host.CreateApplicationBuilder(args);

var configuration = builder.Configuration;

var ollamaClient = new OllamaApiClient(configuration["OLLAMA_SERVER"]!, configuration["LLM_MODEL"]!);

var kernelBuilder = Kernel.CreateBuilder();
kernelBuilder
    .AddOllamaEmbeddingGenerator(ollamaClient);

var kernel = kernelBuilder.Build();

builder.Services.AddSingleton(kernel);
builder.Services.AddSingleton(ollamaClient);
builder.Services.AddSingleton<DataUploader>();
builder.Services.AddQdrantVectorStore(configuration["QDRANT_HOST"]!, int.Parse(configuration["QDRANT_PORT"]!), false);

builder.Services.AddSingleton(sp => sp.GetRequiredService<Kernel>().GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>());

builder.Services.AddHostedService<Worker>();

await Task.Delay(5000);

var host = builder.Build();
host.Run();