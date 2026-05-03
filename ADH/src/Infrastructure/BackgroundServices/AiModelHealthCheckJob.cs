using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ADH.Application.Interfaces;
using ADH.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ADH.Infrastructure.BackgroundServices;

public class AiModelHealthCheckJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppLogger<AiModelHealthCheckJob> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public AiModelHealthCheckJob(IServiceProvider serviceProvider, IAppLogger<AiModelHealthCheckJob> logger, IHttpClientFactory httpClientFactory)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInfo("AI Model Health Check Job is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckOllamaHealthAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during AI health check.");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task CheckOllamaHealthAsync()
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        IAiProviderManager manager = scope.ServiceProvider.GetRequiredService<IAiProviderManager>();
        IHubContext<ChatHub> hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<ChatHub>>();
        
        HttpClient client = _httpClientFactory.CreateClient("OllamaClient");
        
        try
        {
            HttpResponseMessage response = await client.GetAsync("/api/tags");
            
            if (response.IsSuccessStatusCode)
            {
                if (!manager.IsOllamaHealthy)
                {
                    _logger.LogInfo("Ollama is back online. Switching back to On-Premise.");
                    manager.IsOllamaHealthy = true;
                    manager.SetProvider("Ollama");
                    await hubContext.Clients.All.SendAsync("SystemNotice", "AI System: On-Premise model restored.");
                }
            }
            else
            {
                await HandleFailover(manager, hubContext);
            }
        }
        catch
        {
            await HandleFailover(manager, hubContext);
        }
    }

    private async Task HandleFailover(IAiProviderManager manager, IHubContext<ChatHub> hubContext)
    {
        if (manager.IsOllamaHealthy)
        {
            _logger.LogCritical("Ollama is UNREACHABLE. Initiating failover to OpenAI.");
            manager.IsOllamaHealthy = false;
            manager.SetProvider("OpenAI");
            await hubContext.Clients.All.SendAsync("SystemNotice", "AI System: On-Premise model down. Failing over to Cloud AI.");
        }
    }
}
