using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ADH.Core.Entities;

namespace ADH.Application.Interfaces;

public interface IAssetRepository
{
    Task<Asset?> GetByIdAsync(Guid id);
    Task<IEnumerable<Asset>> GetAllAsync();
    Task<IEnumerable<Asset>> GetByUserIdAsync(Guid userId);
    Task AddAsync(Asset asset);
    Task UpdateAsync(Asset asset);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<AssetType>> GetAssetTypesAsync();
}
