using System;
using MediatR;
using ADH.Core.Entities;

namespace ADH.Application.Features.Tickets.Commands;

public record CreateTicketCommand(string Description, Guid UserId) : IRequest<Ticket>;
