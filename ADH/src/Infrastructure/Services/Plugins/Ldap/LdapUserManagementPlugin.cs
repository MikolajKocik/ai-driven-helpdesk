using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Threading.Tasks;
using ADH.Core.Interfaces;
using System.Linq;

namespace ADH.Infrastructure.Services.Plugins.Ldap;

public sealed class LdapUserManagementPlugin
{
    private readonly ILdapService _ldapService;

    public LdapUserManagementPlugin(ILdapService ldapService)
    {
        _ldapService = ldapService;
    }

    [KernelFunction, Description("Gets detailed profile information for a specific user.")]
    public async Task<string> GetUserProfile(
        [Description("The username to retrieve")] string username)
    {
        var users = await _ldapService.GetUsersAsync($"(sAMAccountName={username})");
        var user = users.FirstOrDefault();
        
        if (user == null) return "User not found.";

        return $"Name: {user.DisplayName}\nEmail: {user.Email}\nGroups: {string.Join(", ", user.Groups)}";
    }
}
