using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace Application.Features.Tickets.Commands;

public record ProcessJiraWebhookCommand(string Payload) : IRequest;