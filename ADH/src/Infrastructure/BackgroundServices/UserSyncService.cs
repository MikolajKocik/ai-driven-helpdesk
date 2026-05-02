using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ADH.Core.Entities;
using ADH.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ADH.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that periodically syncs users from LDAP/AD to the local database.
/// Includes SmartGroupsMapping to assign roles based on AD groups.
/// </summary>
public class UserSyncService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppLogger<UserSyncService> _logger;

    public UserSyncService(IServiceProvider serviceProvider, IAppLogger<UserSyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInfo("LDAP User Sync Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncUsersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during LDAP user sync.");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task SyncUsersAsync()
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        ILdapService ldapService = scope.ServiceProvider.GetRequiredService<ILdapService>();
        IUserRepository userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        _logger.LogInfo("Starting LDAP user synchronization...");

        IEnumerable<LdapUserDto> ldapUsers = await ldapService.GetUsersAsync("(objectClass=user)");

        foreach (LdapUserDto ldapUser in ldapUsers)
        {
            AppUser? existingUser = await userRepository.GetByUsernameAsync(ldapUser.Username);
            
            if (existingUser == null)
            {
                _logger.LogInfo("Provisioning new user from LDAP: {Username}", ldapUser.Username);
                AppUser newUser = new AppUser
                {
                    Username = ldapUser.Username,
                    PasswordHash = "LDAP_AUTH",
                    Role = MapGroupsToRole(ldapUser.Groups)
                };
                await userRepository.AddAsync(newUser);
            }
            else
            {
                // Update role if changed in AD
                string newRole = MapGroupsToRole(ldapUser.Groups);
                if (existingUser.Role != newRole)
                {
                    _logger.LogInfo("Updating role for {Username}: {Old} -> {New}", existingUser.Username, existingUser.Role, newRole);
                    existingUser.Role = newRole;
                    await userRepository.UpdateAsync(existingUser);
                }
            }
        }

        _logger.LogInfo("LDAP user synchronization completed.");
    }

    private string MapGroupsToRole(IEnumerable<string> adGroups)
    {
        // SmartGroupsMapping Logic
        if (adGroups.Any(g => g.Contains("CN=Jira-Admins") || g.Contains("CN=Domain Admins")))
            return "Admin";
            
        if (adGroups.Any(g => g.Contains("CN=Helpdesk-Agents") || g.Contains("CN=IT-Staff")))
            return "Agent";

        return "Client";
    }
}
