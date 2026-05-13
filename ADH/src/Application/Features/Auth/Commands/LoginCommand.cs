using MediatR;
using ADH.Application.DTOs;

namespace ADH.Application.Features.Auth.Commands;

public record LoginCommand(LoginRequest Request) : IRequest<LoginResponse>;
