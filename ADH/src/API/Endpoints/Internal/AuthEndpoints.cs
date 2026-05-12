using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ADH.Application.Interfaces;
using ADH.Core.Entities;
using ADH.Application.DTOs;
using ADH.API.Filters;
using ADH.API.Helpers;

namespace ADH.API.Endpoints.Internal;

/// <summary>
/// Endpoints for managing user authentication with production-ready security.
/// </summary>
public static class AuthEndpoints
{
    /// <summary>
    /// Maps authentication-related endpoints.
    /// </summary>
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("auth");

        group.MapPost("/login", async (LoginRequest request, IUserRepository repo, IJwtService jwtService, ILdapService ldapService, IAppLogger<Program> logger, CancellationToken cancellationToken) =>
        {
            AppUser? user = await repo.GetByUsernameAsync(request.Username, cancellationToken);
            
            bool isAuthenticated = false;

            if (user != null && user.PasswordHash != "LDAP_AUTH")
            {
                // Try local authentication
                isAuthenticated = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            }
            
            if (!isAuthenticated)
            {
                // Try LDAP authentication
                if (ldapService.Authenticate(request.Username, request.Password))
                {
                    isAuthenticated = true;
                    
                    if (user == null)
                    {
                        // JIT Provisioning
                        logger.LogInfo("JIT Provisioning user {Username} via LDAP", request.Username);
                        user = new AppUser
                        {
                            Username = request.Username,
                            PasswordHash = "LDAP_AUTH"
                        };
                        await repo.AddAsync(user, cancellationToken);
                    }
                }
            }

            if (!isAuthenticated || user == null)
            {
                return Results.Unauthorized();
            }

            string token = jwtService.GenerateToken(user);
            
            return Results.Ok(new 
            { 
                Token = token, 
                User = new { user.Id, user.Username } 
            });
        })
        .AddEndpointFilter<ValidationFilter<LoginRequest>>();

        group.MapPost("/register", async (RegisterRequest request, IUserRepository repo, CancellationToken cancellationToken) =>
        {
            AppUser? existing = await repo.GetByUsernameAsync(request.Username, cancellationToken);
            if (existing != null) return Results.Conflict("User already exists.");

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            
            AppUser newUser = new AppUser
            {
                Username = request.Username,
                PasswordHash = passwordHash
            };

            await repo.AddAsync(newUser, cancellationToken);
            return Results.Created($"/api/v{ApiHelper.MajorVersion}/auth/user/{newUser.Id}", new { newUser.Id, newUser.Username });
        })
        .AddEndpointFilter<ValidationFilter<RegisterRequest>>();
    }
}
