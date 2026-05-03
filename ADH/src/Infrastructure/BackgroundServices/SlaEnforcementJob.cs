using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ADH.Core.Entities;
using ADH.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ADH.Infrastructure.BackgroundServices;

public sealed class SlaEnforcementJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppLogger<SlaEnforcementJob> _logger;

    public SlaEnforcementJob(IServiceProvider serviceProvider, IAppLogger<SlaEnforcementJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInfo("SLA Enforcement Job is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await EnforceSlaAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during SLA enforcement.");
            }

            // Run every minute for precise enforcement
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task EnforceSlaAsync()
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        ITicketRepository ticketRepo = scope.ServiceProvider.GetRequiredService<ITicketRepository>();
        ISlaPolicyRepository slaRepo = scope.ServiceProvider.GetRequiredService<ISlaPolicyRepository>();

        IEnumerable<Ticket> openTickets = await ticketRepo.GetAllAsync();
        IEnumerable<SlaPolicy> policies = await slaRepo.GetAllAsync();
        
        Dictionary<string, SlaPolicy> policyMap = policies.ToDictionary(p => p.Priority);

        foreach (Ticket ticket in openTickets.Where(t => !t.IsResolved))
        {
            bool changed = false;

            // 1. Assign SLA if missing
            if (ticket.SlaResolutionDeadline == null && policyMap.TryGetValue(ticket.Priority, out SlaPolicy? policy))
            {
                ticket.SlaResponseDeadline = ticket.CreatedAt.AddMinutes(policy.ResponseTimeMinutes);
                ticket.SlaResolutionDeadline = ticket.CreatedAt.AddMinutes(policy.ResolutionTimeMinutes);
                changed = true;
                _logger.LogInfo("Assigned SLA to ticket {Id} based on priority {Priority}", ticket.Id, ticket.Priority);
            }

            // 2. Check for violations
            if (ticket.SlaResolutionDeadline != null)
            {
                DateTime now = DateTime.UtcNow;
                string newStatus = "InThreshold";

                if (now > ticket.SlaResolutionDeadline)
                {
                    newStatus = "Violated";
                }
                else if (now.AddMinutes(15) > ticket.SlaResolutionDeadline)
                {
                    newStatus = "ApproachingThreshold";
                }

                if (ticket.SlaStatus != newStatus)
                {
                    ticket.SlaStatus = newStatus;
                    changed = true;
                    _logger.LogWarning("SLA Status change for ticket {Id}: {Status}", ticket.Id, newStatus);
                }
            }

            if (changed)
            {
                await ticketRepo.UpdateAsync(ticket);
            }
        }
    }
}
