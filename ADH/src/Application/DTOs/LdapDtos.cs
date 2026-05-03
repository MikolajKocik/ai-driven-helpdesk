using System.Collections.Generic;

namespace ADH.Application.DTOs;

public record LdapAssetDto(string Name, string OperatingSystem, string SerialNumber);

public record LdapUserDto(string Username, string Email, string DisplayName)
{
    public IEnumerable<string> Groups { get; init; } = new List<string>();
}
