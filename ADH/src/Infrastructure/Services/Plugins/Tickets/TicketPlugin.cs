using Microsoft.SemanticKernel;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ADH.Core.Entities;
using ADH.Application.Interfaces;

namespace ADH.Infrastructure.Services.Plugins.Tickets;

public sealed class TicketPlugin
{
    private readonly ITicketRepository _ticketRepo;

    public TicketPlugin(ITicketRepository ticketRepo)
    {
        _ticketRepo = ticketRepo;
    }

    [KernelFunction, Description("Creates a new support ticket.")]
    public async Task<string> CreateTicket(string description, CancellationToken cancellationToken = default)
    {
        var ticket = new Ticket { Description = description };
        await _ticketRepo.AddAsync(ticket, cancellationToken);
        return $"Ticket {ticket.Id} created.";
    }
}
