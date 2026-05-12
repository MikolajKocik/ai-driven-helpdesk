using System;
using System.Threading;
using System.Threading.Tasks;
using ADH.Application.Interfaces;
using ADH.Infrastructure.Services.Assets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ADH.Infrastructure.BackgroundServices;

public sealed class AssetDiscoveryJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppLogger<AssetDiscoveryJob> _logger;

    public AssetDiscoveryJob(IServiceProvider serviceProvider, IAppLogger<AssetDiscoveryJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInfo("Asset Discovery Job is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    // Run all discovery services
                    var discoveryServices = scope.ServiceProvider.GetServices<IAssetDiscoveryService>();
                    foreach (var service in discoveryServices)
                    {
                        await service.DiscoverNewAssetsAsync(stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Asset Discovery process.");
            }

            // Run discovery once per day
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}
