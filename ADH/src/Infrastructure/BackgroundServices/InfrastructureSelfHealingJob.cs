using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ADH.Core.Interfaces;
using ADH.Infrastructure.Services.Plugins.System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ADH.Infrastructure.BackgroundServices;

public sealed class InfrastructureSelfHealingJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppLogger<InfrastructureSelfHealingJob> _logger;

    public InfrastructureSelfHealingJob(IServiceProvider serviceProvider, IAppLogger<InfrastructureSelfHealingJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInfo("Infrastructure Self-Healing Job is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MonitorAndHealAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during self-healing monitor.");
            }

            // Check every 5 minutes
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task MonitorAndHealAsync()
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        LocalAutomationPlugin automation = scope.ServiceProvider.GetRequiredService<LocalAutomationPlugin>();
        
        // Example: Monitor Disk Space
        DriveInfo[] drives = DriveInfo.GetDrives();
        DriveInfo mainDrive = drives.FirstOrDefault(d => d.IsReady) ?? drives[0];
        
        double usedPercentage = (1.0 - (double)mainDrive.AvailableFreeSpace / mainDrive.TotalSize) * 100;

        if (usedPercentage > 90.0)
        {
            _logger.LogCritical("CRITICAL: Disk usage at {Percentage}%. Triggering Self-Healing cleanup...", usedPercentage);
            string result = await automation.CleanTempFiles();
            _logger.LogInfo("Self-Healing Result: {Result}", result);
        }
    }
}
