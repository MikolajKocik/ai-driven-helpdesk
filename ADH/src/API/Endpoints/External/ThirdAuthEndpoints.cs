using Google.Apis.Auth;
using ADH.Application.DTOs;
using ADH.Application.Interfaces;
using ADH.Core.Entities;

namespace ADH.API.Endpoints.External;

public static class ThirdAuthEndpoints
{
    public static void MapThirdAuthEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder googleGroup = app.MapGroup("auth/google");
        
        googleGroup.MapPost("/callback", async (GoogleAuthRequest request, IUserRepository repo, IJwtService jwtService, IConfiguration cfg, CancellationToken cancellationToken) =>
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new [] { cfg["Authentication:Google:ClientId"] }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);

                string email = payload.Email;
                string name = payload.Name ?? payload.Email;

                AppUser? user = await repo.GetByUsernameAsync(email, cancellationToken);
                if (user is null)
                {
                    user = new AppUser
                    {
                        Username = email,
                        Email = email,
                        DisplayName = name,
                        Role = "Client"
                    };
                    await repo.AddAsync(user, cancellationToken);
                }

                var token = jwtService.GenerateToken(user);

                return Results.Ok(new SocialAuthResponse(
                    token, user.Id.ToString(), 
                    user.Username, 
                    user.Email ?? ""
                ));
            }
            catch (InvalidJwtException ex)
            {
                return Results.BadRequest(new { Message = "Invalid Google token", Error = ex.Message });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });
    }
}
