using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Threading.Tasks;
using ADH.Application.Interfaces;
using ADH.Application.DTOs;
using System.Collections.Generic;

namespace ADH.Infrastructure.Services.Plugins.Ldap;

public sealed class LdapSearchPlugin
{
    private readonly ILdapService _ldapService;

    public LdapSearchPlugin(ILdapService ldapService)
    {
        _ldapService = ldapService;
    }

    [KernelFunction, Description("Searches for users in Active Directory by a specific filter.")]
    public async Task<IEnumerable<LdapUserDto>> SearchUsers(
        [Description("LDAP search filter, e.g., (sAMAccountName=jdoe) or (mail=*@company.com)")] string filter)
    {
        return await _ldapService.GetUsersAsync(filter);
    }

    [KernelFunction, Description("Finds all users belonging to a specific AD group.")]
    public async Task<IEnumerable<LdapUserDto>> FindUsersInGroup(
        [Description("Distinguished Name of the group")] string groupDn)
    {
        string filter = $"(memberOf={groupDn})";
        return await _ldapService.GetUsersAsync(filter);
    }
}
