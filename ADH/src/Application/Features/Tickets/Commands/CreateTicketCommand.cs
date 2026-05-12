using System;
using MediatR;
using Application.DTOs;

namespace ADH.Application.Features.Tickets.Commands;

public record CreateTicketCommand(JiraWorkItem WorkItem) : IRequest;
