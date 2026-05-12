using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ADH.Core.Entities;

namespace ADH.Application.Interfaces;

public interface IHelpArticleRepository
{
    Task<HelpArticle?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<HelpArticle>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(HelpArticle article, CancellationToken cancellationToken);
    Task UpdateAsync(HelpArticle article, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    
    Task<IEnumerable<HelpArticle>> SearchSimilarAsync(float[] queryEmbedding, double matchThreshold, int matchCount, CancellationToken cancellationToken);
    Task<IEnumerable<HelpArticle>> SearchByTextAsync(string query, CancellationToken cancellationToken);
}
