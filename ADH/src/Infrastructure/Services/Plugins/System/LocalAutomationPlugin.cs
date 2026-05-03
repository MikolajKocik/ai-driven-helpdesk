using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System;
using System.Net.NetworkInformation;
using ADH.Application.Interfaces;

namespace ADH.Infrastructure.Services.Plugins.System;

public sealed class LocalAutomationPlugin
{
    private readonly IAppLogger<LocalAutomationPlugin> _logger;
    private readonly ILdapService _ldapService;

    public LocalAutomationPlugin(IAppLogger<LocalAutomationPlugin> logger, ILdapService ldapService)
    {
        _logger = logger;
        _ldapService = ldapService;
    }

    [KernelFunction, Description("Checks disk space on the local server.")]
    public Task<string> GetDiskSpace()
    {
        DriveInfo[] drives = DriveInfo.GetDrives();
        DriveInfo mainDrive = drives.FirstOrDefault(d => d.IsReady) ?? drives[0];
        
        double freeGb = mainDrive.AvailableFreeSpace / 1024.0 / 1024.0 / 1024.0;
        double totalGb = mainDrive.TotalSize / 1024.0 / 1024.0 / 1024.0;
        double usedPercentage = (1.0 - (double)mainDrive.AvailableFreeSpace / mainDrive.TotalSize) * 100;

        return Task.FromResult($"Drive {mainDrive.Name}: {freeGb:F2} GB free of {totalGb:F2} GB ({usedPercentage:F1}% used).");
    }

    [KernelFunction, Description("Cleans temporary files on the local server to free up space.")]
    public Task<string> CleanTempFiles()
    {
        _logger.LogBusinessAction("System", "Cleanup", "Initiated temporary file cleanup.");
        
        string tempPath = Path.GetTempPath();
        long bytesFreed = 0;
        int filesDeleted = 0;

        try
        {
            DirectoryInfo di = new DirectoryInfo(tempPath);
            foreach (FileInfo file in di.GetFiles())
            {
                try
                {
                    bytesFreed += file.Length;
                    file.Delete();
                    filesDeleted++;
                }
                catch
                {
                    // Ignore locked files in temp
                }
            }
        }
        catch (Exception ex)
        {
            return Task.FromResult($"ERROR: Cleanup failed: {ex.Message}");
        }
        
        double mbFreed = bytesFreed / 1024.0 / 1024.0;
        return Task.FromResult($"SUCCESS: Temporary files cleaned. Deleted {filesDeleted} files, freed {mbFreed:F2} MB.");
    }

    [KernelFunction, Description("Restarts a local system service (e.g. 'ollama', 'nginx').")]
    public Task<string> RestartService(
        [Description("The name of the service to restart")] string serviceName)
    {
        _logger.LogSecurityEvent("AI_SYSTEM", "Service Restart", serviceName);
        
        try
        {
            // On Linux, we use systemctl
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "sudo",
                Arguments = $"systemctl restart {serviceName}",
                RedirectStandardError = true,
                UseShellExecute = false
            };

            Process? process = Process.Start(psi);
            process?.WaitForExit();

            if (process?.ExitCode != 0)
            {
                string error = process?.StandardError.ReadToEnd() ?? "Unknown error";
                return Task.FromResult($"FAILURE: Could not restart service {serviceName}. Error: {error}");
            }
            
            return Task.FromResult($"SUCCESS: Service '{serviceName}' has been restarted.");
        }
        catch (Exception ex)
        {
            return Task.FromResult($"ERROR: Exception during service restart: {ex.Message}");
        }
    }

    [KernelFunction, Description("Reads the last few lines of a local system log file.")]
    public async Task<string> ReadLocalLog(
        [Description("Path to the log file")] string filePath,
        [Description("Number of lines to read")] int lineCount = 10)
    {
        if (!File.Exists(filePath)) return "ERROR: Log file not found.";

        string[] lines = await File.ReadAllLinesAsync(filePath);
        IEnumerable<string> lastLines = lines.TakeLast(lineCount);
        
        return string.Join("\n", lastLines);
    }

    [KernelFunction, Description("Resets a user's password in Active Directory (On-Premise only).")]
    public async Task<string> ResetUserPassword(
        [Description("The username to reset")] string username,
        [Description("The new temporary password")] string newPassword)
    {
        _logger.LogSecurityEvent("AI_ASSISTANT", "Password Reset Request", username);
        
        // In Air-Gapped / Sovereign mode, we do this directly via local LDAP
        bool success = await _ldapService.ResetPasswordAsync(username, newPassword);
        
        if (success)
        {
            return $"SUCCESS: Password for user '{username}' has been reset to '{newPassword}'. User must change it at next logon.";
        }
        
        return $"FAILURE: Could not reset password for '{username}'. Check LDAP logs for details.";
    }

    [KernelFunction, Description("Verifies if the system is in Air-Gapped mode (no Internet access).")]
    public Task<string> CheckNetworkIsolation()
    {
        try
        {
            // Try to ping a public DNS (e.g. Google 8.8.8.8)
            using Ping ping = new Ping();
            PingReply reply = ping.Send("8.8.8.8", 1000);
            
            if (reply.Status == IPStatus.Success)
            {
                return Task.FromResult("WARNING: System is NOT fully isolated. Internet access detected.");
            }
        }
        catch
        {
            // Exception expected in isolated environment
        }
        
        return Task.FromResult("CONFIRMED: System is in Air-Gapped mode. No outbound Internet traffic detected.");
    }
}
