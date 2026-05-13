using MediatR;
using ADH.Core.Entities;
using ADH.Application.Interfaces;
using ADH.Application.DTOs;
using BCryptTool = BCrypt.Net.BCrypt;

namespace ADH.Application.Features.Auth.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly ILdapService _ldapService;
    private readonly IAppLogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IJwtService jwtService,
        ILdapService ldapService,
        IAppLogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _ldapService = ldapService;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        AppUser? user = await _userRepository.GetByUsernameAsync(request.Request.Username, cancellationToken);
        
        bool isAuthenticated = false;

        if (user != null && user.PasswordHash != "LDAP_AUTH")
        {
            // Try local authentication
            isAuthenticated = BCryptTool.Verify(request.Request.Password, user.PasswordHash);
        }
        
        if (!isAuthenticated)
        {
            // Try LDAP authentication
            if (_ldapService.Authenticate(request.Request.Username, request.Request.Password))
            {
                isAuthenticated = true;
                
                if (user == null)
                {
                    // JIT Provisioning
                    _logger.LogInfo("JIT Provisioning user {Username} via LDAP", request.Request.Username);
                    user = new AppUser
                    {
                        Username = request.Request.Username,
                        PasswordHash = "LDAP_AUTH"
                    };
                    await _userRepository.AddAsync(user, cancellationToken);
                }
            }
        }

        if (!isAuthenticated || user == null)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        string token = _jwtService.GenerateToken(user);
        
        return new LoginResponse(
            Token: token,
            User: new { user.Id, user.Username }
        );
    }
}
