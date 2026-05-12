using System.Collections.Generic;
using System.Threading.Tasks;
using ADH.Core.Entities;
using ADH.Application.Interfaces;
using ADH.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ADH.Infrastructure.Repositories;

public sealed class SlaPolicyRepository : BaseRepository<SlaPolicy, ApplicationDbContext>, ISlaPolicyRepository
{
    public SlaPolicyRepository(ApplicationDbContext context, IAppLogger<SlaPolicy> logger, ICurrentUserService currentUserService) 
        : base(context, logger, currentUserService)
    {
    }

    public override async Task<IEnumerable<SlaPolicy>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await Context.SlaPolicies.ToListAsync(cancellationToken);
    }

    public async Task<SlaPolicy?> GetByPriorityAsync(string priority, CancellationToken cancellationToken)
    {
        return await Context.SlaPolicies.FirstOrDefaultAsync(p => p.Priority == priority, cancellationToken);
    }
}
