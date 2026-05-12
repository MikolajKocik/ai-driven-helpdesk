using ADH.Core.Entities;
using ADH.Application.Interfaces;
using ADH.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ADH.Infrastructure.Repositories;

/// <summary>
/// Repository for managing support tickets with structured logging.
/// </summary>
public sealed class TicketRepository : BaseRepository<Ticket, ApplicationDbContext>, ITicketRepository
{
    public TicketRepository(ApplicationDbContext context, IAppLogger<Ticket> logger, ICurrentUserService currentUserService) 
        : base(context, logger, currentUserService)
    {
    }

    /// <summary>
    /// Retrieves a ticket by its ID, including the associated user information.
    /// </summary>
    public override async Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await Context.Tickets.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves all tickets for a specific user, ordered by creation date descending.
    /// </summary>
    public async Task<IEnumerable<Ticket>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await Context.Tickets
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves all tickets in the system, including user info, ordered by creation date descending.
    /// </summary>
    public override async Task<IEnumerable<Ticket>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await Context.Tickets
            .Include(t => t.User)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }
    
    /// <summary>
    /// Get all external tickets to sync their status as fallback every hour if webhook failed
    /// </summary>    
    public async Task<IEnumerable<Ticket>> GetActiveExternalTicketsAsync(string systemName, CancellationToken cancellationToken)
    {
        return await Context.Tickets
            .Where(t => t.ExternalSystem == systemName && t.Status != "Resolved" && t.Status != "Closed")
            .ToListAsync(cancellationToken);
    }
}
