using ADH.Infrastructure.Hubs;
using Application.Features.Tickets.Commands;
using Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundServices;

public sealed class JiraWebhookProcessor : BackgroundService
{
    private readonly IWebhookQueue _webhookQueue;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ILogger<JiraWebhookProcessor> _logger; 

    public JiraWebhookProcessor(
        IWebhookQueue webhookQueue,
        IServiceProvider serviceProvider,
        IHubContext<ChatHub> hubContext,
        ILogger<JiraWebhookProcessor> logger
        )
    {
        _webhookQueue = webhookQueue;
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                string payload = await _webhookQueue.DequeueAsync(stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    
                bool wasUpdated = await mediator.Send(new ProcessJiraWebhookCommand(payload), stoppingToken);
                
                if (wasUpdated)
                {
                    _logger.LogInformation("Broadcasting 'TicketUpdated' event to all connected clients.");
                    await _hubContext.Clients.All.SendAsync("TicketUpdated", cancellationToken: stoppingToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("JiraWebhookProcessor is stopping cleanly.");  
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured during processing background webhook.");
        }
    }
}