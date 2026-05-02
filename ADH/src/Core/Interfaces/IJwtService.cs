using ADH.Core.Entities;

namespace ADH.Core.Interfaces;

/// <summary>
/// Service for generating JWT tokens for authenticated users.
/// </summary>
public interface IJwtService
{
    string GenerateToken(AppUser user);
}
