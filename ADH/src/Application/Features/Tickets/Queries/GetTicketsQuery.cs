using System;
using System.Collections.Generic;
using MediatR;
using ADH.Core.Entities;

namespace ADH.Application.Features.Tickets.Queries;

public record GetTicketsQuery(Guid UserId) : IRequest<IEnumerable<Ticket>>;
