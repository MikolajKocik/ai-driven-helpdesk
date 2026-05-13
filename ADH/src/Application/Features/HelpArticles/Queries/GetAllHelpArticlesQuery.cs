using System.Collections.Generic;
using MediatR;
using ADH.Core.Entities;

namespace ADH.Application.Features.HelpArticles.Queries;

public record GetAllHelpArticlesQuery() : IRequest<IEnumerable<object>>;
