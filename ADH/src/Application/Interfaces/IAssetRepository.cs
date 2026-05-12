using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ADH.Core.Entities;

namespace ADH.Application.Interfaces;

public interface IAssetRepository
{
    Task<Asset?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<Asset>> GetAllAsync(CancellationToken cancellationToken);
    Task<IEnumerable<Asset>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task AddAsync(Asset asset, CancellationToken cancellationToken);
    Task UpdateAsync(Asset asset, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<AssetType>> GetAssetTypesAsync(CancellationToken cancellationToken);
}
