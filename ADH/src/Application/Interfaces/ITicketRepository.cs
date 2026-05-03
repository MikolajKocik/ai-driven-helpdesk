using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ADH.Core.Entities;

namespace ADH.Application.Interfaces;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid id);
    Task<IEnumerable<Ticket>> GetAllForUserAsync(Guid userId);
    Task<IEnumerable<Ticket>> GetAllAsync();
    Task AddAsync(Ticket ticket);
    Task UpdateAsync(Ticket ticket);
}
