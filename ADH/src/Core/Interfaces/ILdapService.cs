using System.Collections.Generic;
using System.Threading.Tasks;

namespace ADH.Core.Interfaces;

public interface ILdapService
{
    bool Authenticate(string username, string password);
    Task<IEnumerable<LdapUserDto>> GetUsersAsync(string searchFilter);
    Task<IEnumerable<LdapUserDto>> GetUsersFromPartnerAsync(string partnerName, string searchFilter);
    Task<bool> UnlockUserAsync(string username);
    Task<bool> ResetPasswordAsync(string username, string newPassword);
}

public record LdapUserDto(string Username, string Email, string DisplayName)
{
    public IEnumerable<string> Groups { get; init; } = new List<string>();
}
