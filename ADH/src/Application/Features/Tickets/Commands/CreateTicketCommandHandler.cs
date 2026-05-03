using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ADH.Core.Entities;
using ADH.Application.Interfaces;

namespace ADH.Application.Features.Tickets.Commands;

public class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, Ticket>
{
    private readonly ITicketRepository _ticketRepository;

    public CreateTicketCommandHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Ticket> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = new Ticket
        {
            Description = request.Description,
            UserId = request.UserId
        };
        
        await _ticketRepository.AddAsync(ticket);
        
        return ticket;
    }
}
