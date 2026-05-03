using ADH.Core.Entities;

namespace ADH.Application.Interfaces;

/// <summary>
/// Service for generating JWT tokens for authenticated users.
/// </summary>
public interface IJwtService
{
    string GenerateToken(AppUser user);
}
