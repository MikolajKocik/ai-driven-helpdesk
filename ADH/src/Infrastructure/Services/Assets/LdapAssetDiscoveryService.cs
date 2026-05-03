using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ADH.Core.Entities;
using ADH.Application.Interfaces;
using ADH.Application.DTOs;

namespace ADH.Infrastructure.Services.Assets;

public sealed class LdapAssetDiscoveryService : IAssetDiscoveryService
{
    private readonly ILdapService _ldapService;
    private readonly IAssetRepository _assetRepo;
    private readonly IAppLogger<LdapAssetDiscoveryService> _logger;

    public LdapAssetDiscoveryService(ILdapService ldapService, IAssetRepository assetRepo, IAppLogger<LdapAssetDiscoveryService> logger)
    {
        _ldapService = ldapService;
        _assetRepo = assetRepo;
        _logger = logger;
    }

    public async Task<int> DiscoverNewAssetsAsync()
    {
        _logger.LogInfo("Starting LDAP Asset Discovery...");
        IEnumerable<LdapAssetDto> ldapComputers = await _ldapService.GetComputersAsync();
        HashSet<string> existingAssets = (await _assetRepo.GetAllAsync()).Select(a => a.SerialNumber).ToHashSet();
        IEnumerable<AssetType> assetTypes = await _assetRepo.GetAssetTypesAsync();
        AssetType computerType = assetTypes.FirstOrDefault(t => t.Name == "Computer") ?? assetTypes.First();

        int importedCount = 0;
        foreach (LdapAssetDto ldapComp in ldapComputers)
        {
            if (!existingAssets.Contains(ldapComp.SerialNumber))
            {
                Asset newAsset = new Asset
                {
                    Name = ldapComp.Name,
                    SerialNumber = ldapComp.SerialNumber,
                    Model = ldapComp.OperatingSystem,
                    AssetTypeId = computerType.Id,
                    Status = "Active",
                    LastAuditDate = DateTime.UtcNow
                };

                await _assetRepo.AddAsync(newAsset);
                importedCount++;
            }
        }

        _logger.LogInfo("LDAP Asset Discovery finished. Imported {Count} computers.", importedCount);
        return importedCount;
    }
}
