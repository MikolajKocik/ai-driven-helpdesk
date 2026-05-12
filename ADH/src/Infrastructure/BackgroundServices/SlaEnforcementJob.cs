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
                await EnforceSlaAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during SLA enforcement.");
            }

            // Run every minute for precise enforcement
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task EnforceSlaAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        ITicketRepository ticketRepo = scope.ServiceProvider.GetRequiredService<ITicketRepository>();
        ISlaPolicyRepository slaRepo = scope.ServiceProvider.GetRequiredService<ISlaPolicyRepository>();

        IEnumerable<Ticket> openTickets = await ticketRepo.GetAllAsync(cancellationToken);
        IEnumerable<SlaPolicy> policies = await slaRepo.GetAllAsync(cancellationToken);
        
        Dictionary<string, SlaPolicy> policyMap = policies.ToDictionary(p => p.Priority);
        DateTime currentTime = DateTime.UtcNow;

        foreach (Ticket ticket in openTickets.Where(t => !t.IsResolved))
        {
            bool changed = false;

            if (ticket.SlaResolutionDeadline == null && policyMap.TryGetValue(ticket.Priority, out SlaPolicy? policy))
            {
                ticket.ApplySlaPolicy(policy.ResponseTimeMinutes, policy.ResolutionTimeMinutes);
                changed = true;
                _logger.LogInfo("Assigned SLA to ticket {Id} based on priority {Priority}", ticket.Id, ticket.Priority);
            }

            bool slaStatusChanged = ticket.EvaluateSlaStatus(currentTime);
            if (slaStatusChanged)
            {
                changed = true;
                _logger.LogWarning("SLA Status change for ticket {Id}: {Status}", ticket.Id, ticket.SlaStatus);
            }

            if (changed)
            {
                await ticketRepo.UpdateAsync(ticket, cancellationToken);
            }
        }
    }
}
