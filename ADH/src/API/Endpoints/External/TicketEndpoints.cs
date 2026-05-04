using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ADH.Application.Interfaces;
using ADH.Core.Entities;
using ADH.API.Helpers;

namespace ADH.API.Endpoints.External;

/// <summary>
/// Endpoints for users to manage their support tickets.
/// </summary>
public static class TicketEndpoints
{
    /// <summary>
    /// Maps ticket-related endpoints.
    /// </summary>
    public static void MapTicketEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("tickets");

        group.MapGet("/", async (ITicketRepository repo) =>
        {
            IEnumerable<Ticket> tickets = await repo.GetAllAsync();
            return Results.Ok(tickets);
        });

        group.MapPost("/", async (Ticket ticket, ITicketRepository repo) =>
        {
            await repo.AddAsync(ticket);
            return Results.Created($"/api/v{ApiHelper.MajorVersion}/tickets/{ticket.Id}", ticket);
        });
    }
}
