using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ADH.Core.Entities;

namespace ADH.Application.Interfaces;

public interface IHelpArticleRepository
{
    Task<HelpArticle?> GetByIdAsync(Guid id);
    Task<IEnumerable<HelpArticle>> GetAllAsync();
    Task AddAsync(HelpArticle article);
    Task UpdateAsync(HelpArticle article);
    Task DeleteAsync(Guid id);
    
    Task<IEnumerable<HelpArticle>> SearchSimilarAsync(float[] queryEmbedding, double matchThreshold, int matchCount);
    Task<IEnumerable<HelpArticle>> SearchByTextAsync(string query);
}
