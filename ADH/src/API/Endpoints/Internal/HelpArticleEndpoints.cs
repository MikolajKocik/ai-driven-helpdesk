using ADH.Core.Entities;
using ADH.API.Helpers;
using MediatR;
using ADH.Application.Features.HelpArticles.Queries;
using ADH.Application.Features.HelpArticles.Commands;
using Microsoft.AspNetCore.Mvc;

namespace ADH.API.Endpoints.Internal;

/// <summary>
/// Endpoints for managing help articles in the knowledge base.
/// </summary>
public static class HelpArticleEndpoints
{
    /// <summary>
    /// Maps knowledge base management endpoints.
    /// </summary>
    public static void MapHelpArticleEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("articles");

        group.MapGet("/", async ([FromServices] IMediator mediatR, CancellationToken cancellationToken) =>
        {
            IEnumerable<object> articles = await mediatR.Send(new GetAllHelpArticlesQuery(), cancellationToken);
            return Results.Ok(articles);
        });

        group.MapPost("/", async (HelpArticle article, [FromServices] IMediator mediatR, CancellationToken cancellationToken) =>
        {
            object result = await mediatR.Send(new CreateHelpArticleCommand(article), cancellationToken);
            return Results.Created($"/api/v{ApiHelper.MajorVersion}/articles/{article.Id}", result);
        });

        group.MapDelete("/{id}", async (Guid id, [FromServices] IMediator mediatR, CancellationToken cancellationToken) =>
        {
            await mediatR.Send(new DeleteHelpArticleCommand(id), cancellationToken);
            return Results.NoContent();
        });
    }
}
