using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ADH.Core.Entities;
using ADH.Application.Interfaces;
using System.Linq;

namespace ADH.Infrastructure.Services.Assets;

public sealed class NetworkDiscoveryService : IAssetDiscoveryService
{
    private readonly IAssetRepository _assetRepo;
    private readonly IAppLogger<NetworkDiscoveryService> _logger;
    private readonly IConfiguration _configuration;
    private static SemaphoreSlim semaphore = new SemaphoreSlim(20); 

    public NetworkDiscoveryService(IAssetRepository assetRepo, IAppLogger<NetworkDiscoveryService> logger, IConfiguration configuration)
    {
        _assetRepo = assetRepo;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<int> DiscoverNewAssetsAsync()
    {
        _logger.LogInfo("Starting Network Asset Discovery (Ping Scan)...");
        int foundCount = 0;
        
        string[] subnets = _configuration.GetSection("Discovery:Subnets").Get<string[]>() ?? new[] { "127.0.0" };
        int timeout = _configuration.GetValue<int>("Discovery:PingTimeoutMs", 100);

        foreach (string subnet in subnets)
        {
            _logger.LogInfo("Scanning subnet {Subnet}.x...", subnet);
            var pingTasks = new List<Task<PingReply>>();
            
            for (int i = 1; i < 255; i++)
            {
                string ip = $"{subnet}.{i}";
                pingTasks.Add(Task.Run(async () =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        using var ping = new Ping();
                        return await ping.SendPingAsync(ip, timeout);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            PingReply[] results = await Task.WhenAll(pingTasks);
            foundCount += await ProcessDiscoveryResults(results);
        }

        _logger.LogInfo("Network Discovery finished. Total found: {Count} new assets.", foundCount);
        return foundCount;
    }

    private async Task<int> ProcessDiscoveryResults(PingReply[] results)
    {
        int count = 0;
        HashSet<string> existingAssets = (await _assetRepo.GetAllAsync()).Select(a => a.SerialNumber).ToHashSet();
        IEnumerable<AssetType> assetTypes = await _assetRepo.GetAssetTypesAsync();
        AssetType? defaultType = assetTypes.FirstOrDefault(t => t.Name == "Network Device") ?? assetTypes.FirstOrDefault();

        if (defaultType == null)
        {
            _logger.LogWarning("No AssetTypes found in database. Skipping network discovery results.");
            return 0;
        }

        foreach (PingReply reply in results.Where(r => r.Status == IPStatus.Success))
        {
            string ip = reply.Address.ToString();
            if (!existingAssets.Contains(ip))
            {
                Asset newAsset = new Asset
                {
                    Name = $"Discovered-{ip}",
                    SerialNumber = ip,
                    Model = "Auto-Discovered",
                    AssetTypeId = defaultType.Id,
                    Status = "Active",
                    LastAuditDate = DateTime.UtcNow
                };

                await _assetRepo.AddAsync(newAsset);
                count++;
            }
        }
        return count;
    }
}
