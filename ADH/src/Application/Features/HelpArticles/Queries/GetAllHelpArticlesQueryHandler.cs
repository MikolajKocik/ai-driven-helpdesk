using MediatR;
using ADH.Application.Interfaces;
using ADH.Core.Entities;

namespace ADH.Application.Features.HelpArticles.Queries;

public class GetAllHelpArticlesQueryHandler : IRequestHandler<GetAllHelpArticlesQuery, IEnumerable<object>>
{
    private readonly IHelpArticleRepository _helpArticleRepository;

    public GetAllHelpArticlesQueryHandler(IHelpArticleRepository helpArticleRepository)
    {
        _helpArticleRepository = helpArticleRepository;
    }

    public async Task<IEnumerable<object>> Handle(GetAllHelpArticlesQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<HelpArticle> articles = await _helpArticleRepository.GetAllAsync(cancellationToken);
        return articles.Select(a => new { a.Id, a.Title, a.Content });
    }
}
