using MediatR;
using ADH.Core.Entities;

namespace ADH.Application.Features.HelpArticles.Commands;

public record CreateHelpArticleCommand(HelpArticle Article) : IRequest<object>;
