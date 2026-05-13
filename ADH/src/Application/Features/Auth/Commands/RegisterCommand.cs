using MediatR;
using ADH.Application.DTOs;

namespace ADH.Application.Features.Auth.Commands;

public record RegisterCommand(RegisterRequest Request) : IRequest<RegisterResponse>;
