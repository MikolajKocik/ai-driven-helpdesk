using MediatR;
using ADH.Application.Interfaces;
using Microsoft.SemanticKernel.Embeddings;

namespace ADH.Application.Features.HelpArticles.Commands;

public class CreateHelpArticleCommandHandler : IRequestHandler<CreateHelpArticleCommand, object>
{
    private readonly IHelpArticleRepository _helpArticleRepository;
    private readonly ITextEmbeddingGenerationService _embeddingService;

    public CreateHelpArticleCommandHandler(
        IHelpArticleRepository helpArticleRepository,
        ITextEmbeddingGenerationService embeddingService)
    {
        _helpArticleRepository = helpArticleRepository;
        _embeddingService = embeddingService;
    }

    public async Task<object> Handle(CreateHelpArticleCommand request, CancellationToken cancellationToken)
    {
        var generatedEmbeddings = await _embeddingService.GenerateEmbeddingsAsync(
            new List<string> { request.Article.Content }, 
            null, 
            cancellationToken);

        if (generatedEmbeddings.Count > 0)
        {
            request.Article.Embedding = generatedEmbeddings[0].ToArray();
        }
        
        await _helpArticleRepository.AddAsync(request.Article, cancellationToken);
        
        return new { request.Article.Id, request.Article.Title, request.Article.Content };
    }
}
