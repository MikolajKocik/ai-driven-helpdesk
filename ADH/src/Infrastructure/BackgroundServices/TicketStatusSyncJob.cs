using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ADH.Core.Entities;
using ADH.Core.Interfaces;
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
                await SyncStatusesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during ticket status sync.");
            }

            // Sync every 15 minutes
            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }

    private async Task SyncStatusesAsync()
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        ITicketRepository ticketRepo = scope.ServiceProvider.GetRequiredService<ITicketRepository>();
        IJiraService jiraService = scope.ServiceProvider.GetRequiredService<IJiraService>();

        // Only fetch active tickets linked to Jira
        IEnumerable<Ticket> tickets = await ticketRepo.GetAllAsync();
        List<Ticket> jiraTickets = tickets.Where(t => t.ExternalSystem == "Jira" && !t.IsResolved).ToList();
        
        _logger.LogInfo("Syncing {Count} active Jira tickets...", jiraTickets.Count);

        foreach (Ticket ticket in jiraTickets)
        {
            if (string.IsNullOrEmpty(ticket.ExternalId)) continue;

            string externalStatus = await jiraService.GetIssueStatusAsync(ticket.ExternalId);
            
            if (externalStatus != "NOT_FOUND" && externalStatus != "ERROR")
            {
                if (ticket.Status != externalStatus)
                {
                    _logger.LogInfo("Updating ticket {Id} status: {Old} -> {New}", ticket.Id, ticket.Status, externalStatus);
                    ticket.Status = externalStatus;
                    ticket.LastSyncAt = DateTime.UtcNow;
                    await ticketRepo.UpdateAsync(ticket);
                }
            }
        }
    }
}
