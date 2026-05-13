using MediatR;
using ADH.Application.Features.Stats.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ADH.API.Endpoints.Internal;

/// <summary>
/// Endpoints for retrieving system statistics and analytics.
/// </summary>
public static class StatsEndpoints
{
    /// <summary>
    /// Maps system statistics endpoints.
    /// </summary>
    public static void MapStatsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("stats", async ([FromServices] IMediator mediatR, CancellationToken cancellationToken) =>
        {
            object stats = await mediatR.Send(new GetStatsQuery(), cancellationToken);
            return Results.Ok(stats);
        });
    }
}
