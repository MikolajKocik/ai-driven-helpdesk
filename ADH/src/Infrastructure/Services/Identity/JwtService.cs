using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using ADH.Core.Entities;
using ADH.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ADH.Infrastructure.Services.Identity;

/// <summary>
/// Modern implementation of IJwtService using Microsoft.IdentityModel.JsonWebTokens.
/// </summary>
public sealed class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(AppUser user)
    {
        string? secret = _configuration["Jwt:Secret"];
        if (string.IsNullOrEmpty(secret))
            throw new InvalidOperationException("JWT Secret is not configured.");

        byte[] key = Encoding.ASCII.GetBytes(secret);
        
        Dictionary<string, object> claims = new Dictionary<string, object>
        {
            { JwtRegisteredClaimNames.Sub, user.Id.ToString() },
            { JwtRegisteredClaimNames.UniqueName, user.Username },
            { JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString() }
        };

        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Claims = claims,
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        JsonWebTokenHandler tokenHandler = new JsonWebTokenHandler();
        return tokenHandler.CreateToken(tokenDescriptor);
    }
}
