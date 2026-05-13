using MediatR;

namespace ADH.Application.Features.Stats.Queries;

public record GetStatsQuery() : IRequest<object>;
