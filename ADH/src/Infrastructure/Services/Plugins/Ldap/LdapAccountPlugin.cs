using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Threading.Tasks;
using ADH.Core.Interfaces;

namespace ADH.Infrastructure.Services.Plugins.Ldap;

public sealed class LdapAccountPlugin
{
    private readonly ILdapService _ldapService;
    private readonly IAppLogger<LdapAccountPlugin> _logger;

    public LdapAccountPlugin(ILdapService ldapService, IAppLogger<LdapAccountPlugin> logger)
    {
        _ldapService = ldapService;
        _logger = logger;
    }

    [KernelFunction, Description("Unlocks a user account in Active Directory.")]
    public async Task<string> UnlockAccount(
        [Description("The username to unlock")] string username)
    {
        bool success = await _ldapService.UnlockUserAsync(username);
        return success ? $"SUCCESS: Account {username} unlocked." : $"FAILURE: Could not unlock {username}.";
    }

    [KernelFunction, Description("Checks if a user account exists and retrieves its basic status.")]
    public async Task<string> CheckAccountExists(
        [Description("The username to check")] string username)
    {
        var users = await _ldapService.GetUsersAsync($"(sAMAccountName={username})");
        var user = global::System.Linq.Enumerable.FirstOrDefault(users);
        return user != null ? $"User {username} exists (Display: {user.DisplayName})" : $"User {username} not found.";
    }
}
