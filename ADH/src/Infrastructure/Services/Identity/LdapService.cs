using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Threading.Tasks;
using ADH.Application.Interfaces;
using ADH.Application.DTOs;
using Microsoft.Extensions.Configuration;

namespace ADH.Infrastructure.Services.Identity;

public class LdapService : ILdapService
{
    private readonly IConfiguration _configuration;
    private readonly IAppLogger<LdapService> _logger;

    public LdapService(IConfiguration configuration, IAppLogger<LdapService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public bool Authenticate(string username, string password)
    {
        if (AuthenticateInternal(username, password, _configuration.GetSection("Ldap")))
            return true;

        foreach (IConfigurationSection partner in _configuration.GetSection("Ldap:Partners").GetChildren())
        {
            if (AuthenticateInternal(username, password, partner))
            {
                _logger.LogInfo("User {Username} authenticated via Partner LDAP: {Partner}", username, partner.Key);
                return true;
            }
        }

        return false;
    }

    protected virtual bool AuthenticateInternal(string username, string password, IConfigurationSection config)
    {
        try
        {
            string? server = config["Server"];
            string? domain = config["Domain"];
            
            if (string.IsNullOrEmpty(server)) return false;

            using LdapConnection connection = new LdapConnection(new LdapDirectoryIdentifier(server));
            connection.SessionOptions.ProtocolVersion = 3;
            connection.AuthType = AuthType.Basic;
            
            NetworkCredential credential = new NetworkCredential(username, password, domain);
            connection.Bind(credential);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("LDAP Authentication failed for user {Username}: {Message}", username, ex.Message);
            return false;
        }
    }

    public async Task<bool> UnlockUserAsync(string username)
    {
        try
        {
            string? server = _configuration["Ldap:Server"];
            string? searchBase = _configuration["Ldap:SearchBase"];
            
            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(searchBase)) return false;

            using LdapConnection connection = new LdapConnection(new LdapDirectoryIdentifier(server));
            connection.Bind(); // Service account bind

            // 1. Find the User DN
            SearchRequest searchRequest = new SearchRequest(
                searchBase,
                $"(sAMAccountName={username})",
                SearchScope.Subtree,
                "distinguishedName"
            );

            SearchResponse searchResponse = (SearchResponse)await Task.Factory.FromAsync(
                connection.BeginSendRequest, connection.EndSendRequest, searchRequest, PartialResultProcessing.NoPartialResultSupport, null);

            if (searchResponse.Entries.Count == 0) return false;
            string userDn = searchResponse.Entries[0].DistinguishedName;

            // 2. Modify lockoutTime to 0 (Unlocks the account in AD)
            ModifyRequest modifyRequest = new ModifyRequest(userDn, DirectoryAttributeOperation.Replace, "lockoutTime", "0");
            
            DirectoryResponse modifyResponse = await Task.Factory.FromAsync(
                connection.BeginSendRequest, connection.EndSendRequest, modifyRequest, PartialResultProcessing.NoPartialResultSupport, null);

            if (modifyResponse.ResultCode == ResultCode.Success)
            {
                _logger.LogInfo("Successfully unlocked account for {Username} (DN: {DN})", username, userDn);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unlock account in LDAP for {Username}", username);
            return false;
        }
    }

    public async Task<IEnumerable<LdapUserDto>> GetUsersAsync(string searchFilter)
    {
        // Default implementation for primary domain
        return await GetUsersInternalAsync(searchFilter, _configuration.GetSection("Ldap"));
    }

    public async Task<IEnumerable<LdapUserDto>> GetUsersFromPartnerAsync(string partnerName, string searchFilter)
    {
        IConfigurationSection partnerConfig = _configuration.GetSection($"Ldap:Partners:{partnerName}");
        return await GetUsersInternalAsync(searchFilter, partnerConfig);
    }

    private async Task<IEnumerable<LdapUserDto>> GetUsersInternalAsync(string searchFilter, IConfigurationSection config)
    {
        List<LdapUserDto> users = new List<LdapUserDto>();
        try
        {
            string? server = config["Server"];
            string? searchBase = config["SearchBase"];
            
            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(searchBase)) return users;

            using LdapConnection connection = new LdapConnection(new LdapDirectoryIdentifier(server));
            connection.Bind(); 

            SearchRequest request = new SearchRequest(
                searchBase,
                searchFilter,
                SearchScope.Subtree,
                "sAMAccountName", "mail", "displayName", "memberOf"
            );

            SearchResponse response = (SearchResponse)await Task.Factory.FromAsync(
                connection.BeginSendRequest, 
                connection.EndSendRequest, 
                request, 
                PartialResultProcessing.NoPartialResultSupport, 
                null
            );

            foreach (SearchResultEntry entry in response.Entries)
            {
                string username = entry.Attributes["sAMAccountName"]?[0]?.ToString() ?? "";
                string email = entry.Attributes["mail"]?[0]?.ToString() ?? "";
                string displayName = entry.Attributes["displayName"]?[0]?.ToString() ?? "";
                
                // Get groups for Smart Mapping
                List<string> groups = new List<string>();
                DirectoryAttribute? groupAttr = entry.Attributes["memberOf"];
                if (groupAttr != null)
                {
                    foreach (object? val in groupAttr)
                    {
                        groups.Add(val?.ToString() ?? "");
                    }
                }
                
                users.Add(new LdapUserDto(username, email, displayName) { Groups = groups });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LDAP Search failed for {Server}", config["Server"] ?? "Unknown");
        }
        return users;
    }

    public async Task<bool> ResetPasswordAsync(string username, string newPassword)
    {
        _logger.LogSecurityEvent("LDAP", "Password Reset Attempt", username);
        
        IConfigurationSection config = _configuration.GetSection("Ldap");
        string? server = config["Server"];
        if (string.IsNullOrEmpty(server)) return false;

        try
        {
            using LdapConnection connection = new LdapConnection(new LdapDirectoryIdentifier(server));
            connection.SessionOptions.ProtocolVersion = 3;
            // Password reset requires LDAPS and Secure Auth
            connection.SessionOptions.SecureSocketLayer = config.GetValue<bool>("UseSsl", true);
            
            // Use Service Account to perform the reset
            NetworkCredential credential = new NetworkCredential(
                config["ServiceUser"], 
                config["ServicePassword"], 
                config["Domain"]);
            
            connection.Bind(credential);

            // 1. Find the User DN
            SearchRequest searchRequest = new SearchRequest(
                config["SearchBase"],
                $"(sAMAccountName={username})",
                SearchScope.Subtree,
                "distinguishedName");

            SearchResponse searchResponse = (SearchResponse)await Task.Run(() => connection.SendRequest(searchRequest));
            
            if (searchResponse.Entries.Count == 0)
            {
                _logger.LogWarning("LDAP Password Reset: User {Username} not found.", username);
                return false;
            }

            string userDn = searchResponse.Entries[0].DistinguishedName;

            // 2. Prepare the password (Active Directory requirement: UTF-16LE, quoted)
            string quotedPassword = $"\"{newPassword}\"";
            byte[] passwordBytes = System.Text.Encoding.Unicode.GetBytes(quotedPassword);

            // 3. Create ModifyRequest
            ModifyRequest modifyRequest = new ModifyRequest(
                userDn,
                DirectoryAttributeOperation.Replace,
                "unicodePwd",
                passwordBytes);

            // 4. Send request
            DirectoryResponse response = await Task.Run(() => connection.SendRequest(modifyRequest));
            
            if (response.ResultCode == ResultCode.Success)
            {
                _logger.LogInfo("LDAP Password Reset SUCCESS for {Username}", username);
                return true;
            }

            _logger.LogError(null, "LDAP Password Reset FAILED for {Username}. Code: {Code}", username, response.ResultCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during LDAP password reset for {Username}", username);
            return false;
        }
    }

    public async Task<IEnumerable<LdapAssetDto>> GetComputersAsync()
    {
        _logger.LogInfo("Syncing computer objects from LDAP...");
        List<LdapAssetDto> assets = new List<LdapAssetDto>();
        IConfigurationSection config = _configuration.GetSection("Ldap");
        string? server = config["Server"];
        if (string.IsNullOrEmpty(server)) return assets;

        try
        {
            using LdapConnection connection = new LdapConnection(new LdapDirectoryIdentifier(server));
            connection.SessionOptions.ProtocolVersion = 3;
            connection.Bind(new NetworkCredential(config["ServiceUser"], config["ServicePassword"], config["Domain"]));

            SearchRequest searchRequest = new SearchRequest(
                config["SearchBase"],
                "(objectClass=computer)",
                SearchScope.Subtree,
                "cn", "operatingSystem", "serialNumber");

            SearchResponse response = (SearchResponse)await Task.Run(() => connection.SendRequest(searchRequest));

            foreach (SearchResultEntry entry in response.Entries)
            {
                assets.Add(new LdapAssetDto(
                    entry.Attributes["cn"]?[0]?.ToString() ?? "Unknown",
                    entry.Attributes["operatingSystem"]?[0]?.ToString() ?? "Unknown",
                    entry.Attributes["serialNumber"]?[0]?.ToString() ?? Guid.NewGuid().ToString()
                ));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync computers from LDAP");
        }
        return assets;
    }
}
