using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ADH.Core.Entities;
using ADH.Core.Interfaces;
using ADH.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Pgvector;

namespace ADH.Infrastructure.Repositories;

/// <summary>
/// Repository for managing help articles with vector search capabilities and structured logging.
/// </summary>
public sealed class HelpArticleRepository : BaseRepository<HelpArticle, ApplicationDbContext>, IHelpArticleRepository
{
    public HelpArticleRepository(ApplicationDbContext context, IAppLogger<HelpArticle> logger, ICurrentUserService currentUserService) 
        : base(context, logger, currentUserService)
    {
    }

    public async Task<IEnumerable<HelpArticle>> SearchSimilarAsync(float[] queryEmbedding, double matchThreshold, int matchCount)
    {
        string vectorStr = new Vector(queryEmbedding).ToString();

        IQueryable<HelpArticle> query = Context.HelpArticles
            .FromSqlRaw(
                "SELECT id as \"Id\", title as \"Title\", content as \"Content\", null::vector as \"Embedding\" FROM search_help_articles({0}::vector, {1}, {2})",
                vectorStr, matchThreshold, matchCount
            );

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<HelpArticle>> SearchByTextAsync(string query)
    {
        return await Context.HelpArticles
            .Where(a => a.Title.Contains(query) || a.Content.Contains(query))
            .ToListAsync();
    }
}
