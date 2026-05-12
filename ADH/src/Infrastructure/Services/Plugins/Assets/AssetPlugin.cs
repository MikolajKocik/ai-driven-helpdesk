using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ADH.Application.Interfaces;

namespace ADH.Infrastructure.Services.Plugins.Assets;

public sealed class AssetPlugin
{
    private readonly IAssetRepository _assetRepo;

    public AssetPlugin(IAssetRepository assetRepo)
    {
        _assetRepo = assetRepo;
    }

    [KernelFunction, Description("Gets all assets (hardware/software) assigned to a specific user.")]
    public async Task<string> GetUserAssets(
        [Description("The unique ID of the user")] Guid userId,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<Core.Entities.Asset> assets = await _assetRepo.GetByUserIdAsync(userId, cancellationToken);
        if (!assets.Any()) return "No assets found for this user.";

        return string.Join("\n", assets.Select(a => $"- {a.Name} ({a.AssetType?.Name}), Model: {a.Model}, SN: {a.SerialNumber}, Status: {a.Status}"));
    }

    [KernelFunction, Description("Gets details about a specific asset by its ID.")]
    public async Task<string> GetAssetDetails(
        [Description("The unique ID of the asset")] Guid assetId,
        CancellationToken cancellationToken = default)
    {
        Core.Entities.Asset? asset = await _assetRepo.GetByIdAsync(assetId, cancellationToken);
        if (asset == null) return "Asset not found.";

        return $"Asset: {asset.Name}\nType: {asset.AssetType?.Name}\nModel: {asset.Model}\nSN: {asset.SerialNumber}\nOwner: {asset.User?.DisplayName}\nStatus: {asset.Status}";
    }
}
