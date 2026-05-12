using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ADH.Infrastructure.Services.Assets;
using ADH.Application.Interfaces;

namespace ADH.Infrastructure.Services.Plugins.Assets;

public sealed class AssetDiscoveryPlugin
{
    private readonly IEnumerable<IAssetDiscoveryService> _discoveryServices;

    public AssetDiscoveryPlugin(IEnumerable<IAssetDiscoveryService> discoveryServices)
    {
        _discoveryServices = discoveryServices;
    }

    [KernelFunction, Description("Triggers an immediate discovery of new assets in the network and Active Directory.")]
    public async Task<string> TriggerAssetDiscovery(CancellationToken cancellationToken = default)
    {
        int totalFound = 0;
        foreach (IAssetDiscoveryService service in _discoveryServices)
        {
            totalFound += await service.DiscoverNewAssetsAsync(cancellationToken);
        }

        return $"Asset discovery finished. Successfully found and imported {totalFound} new assets into the CMDB.";
    }
}
