using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ADH.Core.Entities;
using ADH.Application.Interfaces;
using Application.Interfaces;
using Application.DTOs;

namespace ADH.Application.Features.Tickets.Commands;

public class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IJiraQueue _queue;
    private readonly ICurrentUserService _userService;

    public CreateTicketCommandHandler(ITicketRepository ticketRepository, IJiraQueue queue, ICurrentUserService userService)
    {
        _ticketRepository = ticketRepository;
        _queue = queue;
        _userService = userService;
    }

    public async Task Handle(CreateTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = new Ticket
        {
            Summary = request.WorkItem.Summary, 
            Description = request.WorkItem.Description,
            UserId = Guid.Parse(_userService.UserId!)
        };
        
        await _ticketRepository.AddAsync(ticket, cancellationToken);
        await _queue.QueueJiraWorkItemAsync(request.WorkItem, cancellationToken);

        await Task.CompletedTask;
    }
}
