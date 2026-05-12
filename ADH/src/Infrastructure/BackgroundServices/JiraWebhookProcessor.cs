using ADH.Application.Interfaces;
using Application.Features.Tickets.Commands;
using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundServices;

public sealed class JiraWebhookProcessor : BackgroundService
{
    private readonly IWebhookQueue _webhookQueue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITicketRepository _ticketRepository;
    private readonly ILogger<JiraWebhookProcessor> _logger; 

    public JiraWebhookProcessor(
        IWebhookQueue webhookQueue,
        IServiceProvider serviceProvider,
        ITicketRepository ticketRepository,
        ILogger<JiraWebhookProcessor> logger
        )
    {
        _webhookQueue = webhookQueue;
        _serviceProvider = serviceProvider;
        _ticketRepository = ticketRepository;
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
                    
                await mediator.Send(new ProcessJiraWebhookCommand(payload), stoppingToken);
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