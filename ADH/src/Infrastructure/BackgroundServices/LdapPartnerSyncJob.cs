using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ADH.Core.Entities;
using ADH.Application.Interfaces;
using ADH.Application.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ADH.Infrastructure.BackgroundServices;

/// <summary>
/// Background job that synchronizes users from Partner LDAP directories.
/// </summary>
public class LdapPartnerSyncJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppLogger<LdapPartnerSyncJob> _logger;
    private readonly IConfiguration _configuration;

    public LdapPartnerSyncJob(IServiceProvider serviceProvider, IAppLogger<LdapPartnerSyncJob> logger, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInfo("LDAP Partner Sync Job is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncPartnerUsersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Partner LDAP sync.");
            }

            // Sync partners every 6 hours
            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }

    private async Task SyncPartnerUsersAsync(CancellationToken stoppingToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        ILdapService ldapService = scope.ServiceProvider.GetRequiredService<ILdapService>();
        IUserRepository userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        IEnumerable<IConfigurationSection> partners = _configuration.GetSection("Ldap:Partners").GetChildren();

        foreach (IConfigurationSection partner in partners)
        {
            _logger.LogInfo("Starting sync for Partner: {Partner}", partner.Key);
            
            IEnumerable<LdapUserDto> users = await ldapService.GetUsersFromPartnerAsync(partner.Key, "(objectClass=user)");

            foreach (LdapUserDto user in users)
            {
                // Logic to handle partner users (e.g. prefixing username with partner name)
                string partnerUsername = $"{partner.Key}\\{user.Username}";
                AppUser? existing = await userRepository.GetByUsernameAsync(partnerUsername, stoppingToken);

                if (existing == null)
                {
                    await userRepository.AddAsync(new AppUser
                    {
                        Username = partnerUsername,
                        PasswordHash = "LDAP_AUTH",
                        Role = "Client", // Partners are usually clients
                        DisplayName = user.DisplayName,
                        Email = user.Email
                    }, stoppingToken);
                }
            }
        }
    }
}
