using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ADH.Core.Entities;
using ADH.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Embeddings;
using System.Collections.Generic;
using Microsoft.Extensions.AI;
using Pgvector;

namespace ADH.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that monitors a local folder and indexes its documents into the vector database.
/// </summary>
public class FileIndexerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _config;
    private readonly ILogger<FileIndexerService> _logger;
    private FileSystemWatcher? _watcher;

    public FileIndexerService(IServiceProvider serviceProvider, IConfiguration config, ILogger<FileIndexerService> logger)
    {
        _serviceProvider = serviceProvider;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Executes the background task of monitoring the directory.
    /// </summary>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string path = _config["LocalDocuments:Path"] ?? "docs_to_index";
        
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        _logger.LogInformation("Starting folder monitoring: {Path}", Path.GetFullPath(path));

        _watcher = new FileSystemWatcher(path);
        _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
        _watcher.Filter = "*.*";
        
        _watcher.Created += async (s, e) => await IndexFileAsync(e.FullPath, stoppingToken);
        _watcher.Changed += async (s, e) => await IndexFileAsync(e.FullPath, stoppingToken);
        
        _watcher.EnableRaisingEvents = true;

        _ = Task.Run(() => InitialScanAsync(path, stoppingToken));

        return Task.CompletedTask;
    }

    private async Task InitialScanAsync(string path, CancellationToken cancellationToken)
    {
        IEnumerable<string> files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".txt") || f.EndsWith(".md"));

        foreach (string file in files)
        {
            await IndexFileAsync(file, cancellationToken);
        }
    }

    private async Task IndexFileAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(filePath)) return;

            string fileName = Path.GetFileName(filePath);
            _logger.LogInformation("Processing local file: {File}", fileName);

            string content = await File.ReadAllTextAsync(filePath);
            if (string.IsNullOrWhiteSpace(content)) return;

            using IServiceScope scope = _serviceProvider.CreateScope();
            IHelpArticleRepository repo = scope.ServiceProvider.GetRequiredService<IHelpArticleRepository>();
            IEmbeddingGenerator<string, Embedding<float>> embeddingService = scope.ServiceProvider.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

            IEnumerable<HelpArticle> allArticles = await repo.GetAllAsync();
            IEnumerable<HelpArticle> oldChunks = allArticles.Where(a => a.Title.StartsWith($"[LOCAL] {fileName}"));
            foreach (HelpArticle oldChunk in oldChunks)
            {
                await repo.DeleteAsync(oldChunk.Id);
            }

            List<string> chunks = SplitText(content, 1000, 200);
            _logger.LogInformation("Split {File} into {Count} chunks.", fileName, chunks.Count);

            for (int i = 0; i < chunks.Count; i++)
            {
                string chunkText = chunks[i];
                Embedding<float> embedding = await embeddingService.GenerateAsync(chunkText, null, cancellationToken);
                
                await repo.AddAsync(new HelpArticle
                {
                    Title = $"[LOCAL] {fileName} (Part {i + 1}/{chunks.Count})",
                    Content = chunkText,
                    Embedding = embedding.Vector
                });
            }

            _logger.LogInformation("Finished indexing file: {File}", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while indexing file {File}", filePath);
        }
    }

    /// <summary>
    /// Simple sliding window text chunker with overlap.
    /// </summary>
    private List<string> SplitText(string text, int chunkSize, int overlap)
    {
        List<string> chunks = new List<string>();
        if (string.IsNullOrEmpty(text)) return chunks;
        if (text.Length <= chunkSize)
        {
            chunks.Add(text);
            return chunks;
        }

        int start = 0;
        while (start < text.Length)
        {
            int end = Math.Min(start + chunkSize, text.Length);
            chunks.Add(text.Substring(start, end - start));
            start += (chunkSize - overlap);
            if (start >= text.Length - overlap) break;
        }
        return chunks;
    }

    public override void Dispose()
    {
        _watcher?.Dispose();
        base.Dispose();
    }
}
