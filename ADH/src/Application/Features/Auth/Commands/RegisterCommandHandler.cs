using MediatR;
using ADH.Core.Entities;
using ADH.Application.Interfaces;
using ADH.Application.DTOs;

namespace ADH.Application.Features.Auth.Commands;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IUserRepository _userRepository;

    public RegisterCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        AppUser? existing = await _userRepository.GetByUsernameAsync(request.Request.Username, cancellationToken);
        if (existing != null)
        {
            throw new InvalidOperationException("User already exists.");
        }

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Request.Password);
        
        AppUser newUser = new AppUser
        {
            Username = request.Request.Username,
            PasswordHash = passwordHash
        };

        await _userRepository.AddAsync(newUser, cancellationToken);
        
        return new RegisterResponse(newUser.Id, newUser.Username);
    }
}
