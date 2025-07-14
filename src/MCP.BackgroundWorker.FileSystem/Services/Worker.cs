namespace MCP.BackgroundWorker.FileSystem.Services;

internal class Worker(ILogger<Worker> logger, DataUploader dataUploader) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            await ReadFilesAsync();
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task ReadFilesAsync()
    {
        var path = "../data";
        foreach (var file in Directory.EnumerateFiles(path))
        {
            if (!new FileInfo(file).Extension.Contains("md"))
            {
                continue;
            }
            var markdown = MarkdownReader.ReadMarkdown(new FileStream(file, FileMode.Open), file);

            await dataUploader.GenerateEmbeddingsAndUpload("documents", markdown);
        }
    }
}
