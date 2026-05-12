using ADH.Core.Entities;
using ADH.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ADH.Infrastructure.BackgroundServices;

/// <summary>
/// Background job that synchronizes ticket statuses from external systems.
/// </summary>
public class TicketStatusSyncJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppLogger<TicketStatusSyncJob> _logger;

    public TicketStatusSyncJob(IServiceProvider serviceProvider, IAppLogger<TicketStatusSyncJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInfo("Ticket Status Sync Job is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncStatusesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during ticket status sync.");
            }

            // Sync every hour
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task SyncStatusesAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        ITicketRepository ticketRepo = scope.ServiceProvider.GetRequiredService<ITicketRepository>();
        IJiraService jiraService = scope.ServiceProvider.GetRequiredService<IJiraService>();

        // Only fetch active tickets linked to Jira
        IEnumerable<Ticket> jiraTickets = await ticketRepo.GetActiveExternalTicketsAsync("JIRA", cancellationToken);

        _logger.LogInfo("Syncing {Count} active Jira tickets...", jiraTickets.Count());

        foreach (Ticket ticket in jiraTickets)
        {
            if (string.IsNullOrEmpty(ticket.ExternalId)) continue;

            string externalStatus = await jiraService.GetIssueStatusAsync(ticket.ExternalId, cancellationToken);
            
            if (externalStatus != "NOT_FOUND" && externalStatus != "ERROR")
            {
                if (ticket.Status != externalStatus)
                {
                    _logger.LogInfo("Updating ticket {Id} status: {Old} -> {New}", ticket.Id, ticket.Status, externalStatus);
        
                    ticket.UpdateStatusFromWebhook(externalStatus);
                    await ticketRepo.UpdateAsync(ticket, cancellationToken);
                }
            }

            await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
        }
    }
}
