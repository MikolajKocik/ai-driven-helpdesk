using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ADH.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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
        app.MapGet("stats", async (ApplicationDbContext context) =>
        {
            int totalTickets = await context.Tickets.CountAsync();
            int resolvedTickets = await context.Tickets.CountAsync(t => t.Status == "Resolved" || t.Status == "Closed");
            int totalArticles = await context.HelpArticles.CountAsync();
            int totalUsers = await context.Users.CountAsync();
            
            return Results.Ok(new 
            { 
                TotalTickets = totalTickets, 
                ResolvedTickets = resolvedTickets, 
                TotalArticles = totalArticles, 
                TotalUsers = totalUsers 
            });
        });
    }
}
