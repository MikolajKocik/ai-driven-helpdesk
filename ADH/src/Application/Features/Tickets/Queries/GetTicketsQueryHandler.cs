using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ADH.Core.Entities;
using ADH.Application.Interfaces;

namespace ADH.Application.Features.Tickets.Queries;

public class GetTicketsQueryHandler : IRequestHandler<GetTicketsQuery, IEnumerable<Ticket>>
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketsQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<IEnumerable<Ticket>> Handle(GetTicketsQuery request, CancellationToken cancellationToken)
    {
        return await _ticketRepository.GetAllForUserAsync(request.UserId);
    }
}
