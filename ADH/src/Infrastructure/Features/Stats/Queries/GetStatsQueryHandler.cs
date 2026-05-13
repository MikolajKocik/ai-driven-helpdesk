using MediatR;
using ADH.Application.Interfaces;
using ADH.Application.Features.Stats.Queries;

namespace ADH.Infrastructure.Features.Stats.Queries;

public sealed class GetStatsQueryHandler : IRequestHandler<GetStatsQuery, object>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IHelpArticleRepository _helpArticleRepository;
    private readonly IUserRepository _userRepository;

    public GetStatsQueryHandler(
        ITicketRepository ticketRepository,
        IHelpArticleRepository helpArticleRepository,
        IUserRepository userRepository)
    {
        _ticketRepository = ticketRepository;
        _helpArticleRepository = helpArticleRepository;
        _userRepository = userRepository;
    }

    public async Task<object> Handle(GetStatsQuery request, CancellationToken cancellationToken)
    {
        int totalTickets = (await _ticketRepository.GetAllAsync(cancellationToken)).Count();
        int resolvedTickets = _ticketRepository.GetResolvedTickets();
        int totalArticles = (await _helpArticleRepository.GetAllAsync(cancellationToken)).Count();
        int totalUsers = (await _userRepository.GetAllAsync(cancellationToken)).Count();
        
        return new 
        { 
            TotalTickets = totalTickets, 
            ResolvedTickets = resolvedTickets, 
            TotalArticles = totalArticles, 
            TotalUsers = totalUsers 
        };
    }
}
