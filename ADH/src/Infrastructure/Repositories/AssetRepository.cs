using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ADH.Core.Entities;
using ADH.Application.Interfaces;
using ADH.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ADH.Infrastructure.Repositories;

public sealed class AssetRepository : BaseRepository<Asset, ApplicationDbContext>, IAssetRepository
{
    public AssetRepository(ApplicationDbContext context, IAppLogger<Asset> logger, ICurrentUserService currentUserService) 
        : base(context, logger, currentUserService)
    {
    }

    public async Task<IEnumerable<Asset>> GetByUserIdAsync(Guid userId)
    {
        return await Context.Assets
            .Include(a => a.AssetType)
            .Where(a => a.UserId == userId)
            .ToListAsync();
    }

    public override async Task<Asset?> GetByIdAsync(Guid id)
    {
        return await Context.Assets
            .Include(a => a.AssetType)
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public override async Task<IEnumerable<Asset>> GetAllAsync()
    {
        return await Context.Assets
            .Include(a => a.AssetType)
            .Include(a => a.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<AssetType>> GetAssetTypesAsync()
    {
        return await Context.AssetTypes.ToListAsync();
    }
}
