using MediatR;

namespace ADH.Application.Features.HelpArticles.Commands;

public record DeleteHelpArticleCommand(Guid Id) : IRequest;
