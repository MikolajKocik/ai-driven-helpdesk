using MediatR;
using ADH.Application.Interfaces;

namespace ADH.Application.Features.HelpArticles.Commands;

public class DeleteHelpArticleCommandHandler : IRequestHandler<DeleteHelpArticleCommand>
{
    private readonly IHelpArticleRepository _helpArticleRepository;

    public DeleteHelpArticleCommandHandler(IHelpArticleRepository helpArticleRepository)
    {
        _helpArticleRepository = helpArticleRepository;
    }

    public async Task Handle(DeleteHelpArticleCommand request, CancellationToken cancellationToken)
    {
        await _helpArticleRepository.DeleteAsync(request.Id, cancellationToken);
    }
}
