using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ADH.Core.Entities;
using ADH.Application.Interfaces;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Extensions.AI;

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

        group.MapGet("/", async (IHelpArticleRepository repo) =>
        {
            IEnumerable<HelpArticle> articles = await repo.GetAllAsync();
            return Results.Ok(articles.Select(a => new { a.Id, a.Title, a.Content }));
        });

        group.MapPost("/", async (
            HelpArticle article, 
            IHelpArticleRepository repo, 
            IEmbeddingGenerator<string, Embedding<float>> embeddingService,
            CancellationToken cancellationToken) =>
        {
            Embedding<float> embedding = await embeddingService.GenerateAsync(article.Content, null, cancellationToken);
            article.Embedding = embedding.Vector;
            
            await repo.AddAsync(article);
            return Results.Created($"/api/articles/{article.Id}", new { article.Id, article.Title, article.Content });
        });

        group.MapDelete("/{id}", async (Guid id, IHelpArticleRepository repo) =>
        {
            await repo.DeleteAsync(id);
            return Results.NoContent();
        });
    }
}
