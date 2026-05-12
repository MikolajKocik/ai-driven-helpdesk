using ADH.Application.Interfaces;
using ADH.Core.Entities;
using ADH.API.Helpers;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ADH.Application.Features.Tickets.Commands;
using System.Text.Json;
using Application.Interfaces;
using ADH.Application.Features.Tickets.Queries;

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

        group.MapGet("/", async ([FromServices] ICurrentUserService userService, [FromServices] IMediator mediatR, CancellationToken cancellationToken) =>
        {
            var tickets = await mediatR.Send(new GetTicketsQuery(Guid.Parse(userService.UserId!)), cancellationToken);
            return Results.Ok(tickets);
        })
        .RequireAuthorization();

        group.MapPost("/", async (JiraWorkItem workItem, [FromServices] IMediator mediatR, CancellationToken cancellationToken) =>
        {
            await mediatR.Send(new CreateTicketCommand(workItem), cancellationToken);
            return Results.Created($"/api/v{ApiHelper.MajorVersion}/tickets/{workItem.Id}", workItem);
        })
        .RequireAuthorization();

        group.MapPost("/webhook", async ([FromBody] JsonElement payload, [FromServices] IWebhookQueue queue, CancellationToken cancellationToken) =>
        {
            string rawJson = payload.GetRawText();
            await queue.EnqueueAsync(rawJson, cancellationToken);

            return Results.Ok();
        });
    }
}
