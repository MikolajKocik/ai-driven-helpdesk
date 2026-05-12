using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ADH.Core.Entities;

namespace ADH.Application.Interfaces;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<Ticket>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<IEnumerable<Ticket>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(Ticket ticket, CancellationToken cancellationToken);
    Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken);
}
