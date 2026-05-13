using ADH.Application.DTOs;
using ADH.API.Filters;
using ADH.API.Helpers;
using MediatR;
using ADH.Application.Features.Auth.Commands;
using Microsoft.AspNetCore.Mvc;

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

        group.MapPost("/login", async (LoginRequest request, [FromServices] IMediator mediatR, CancellationToken cancellationToken) =>
        {
            try
            {
                var response = await mediatR.Send(new LoginCommand(request), cancellationToken);
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
        })
        .AddEndpointFilter<ValidationFilter<LoginRequest>>();

        group.MapPost("/register", async (RegisterRequest request, [FromServices] IMediator mediatR, CancellationToken cancellationToken) =>
        {
            try
            {
                var response = await mediatR.Send(new RegisterCommand(request), cancellationToken);
                return Results.Created($"/api/v{ApiHelper.MajorVersion}/auth/user/{response.Id}", response);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(ex.Message);
            }
        })
        .AddEndpointFilter<ValidationFilter<RegisterRequest>>();
    }
}
