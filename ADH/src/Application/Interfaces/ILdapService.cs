using ADH.Application.DTOs;

namespace ADH.Application.Interfaces;

public interface ILdapService
{
    bool Authenticate(string username, string password);
    Task<IEnumerable<LdapUserDto>> GetUsersAsync(string searchFilter);
    Task<IEnumerable<LdapUserDto>> GetUsersFromPartnerAsync(string partnerName, string searchFilter);
    Task<IEnumerable<LdapAssetDto>> GetComputersAsync();
    Task<bool> UnlockUserAsync(string username);
    Task<bool> ResetPasswordAsync(string username, string newPassword);
}
