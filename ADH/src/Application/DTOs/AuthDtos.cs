namespace ADH.Application.DTOs;

public record LoginRequest(string Username, string Password);
public record RegisterRequest(string Username, string Password, string DisplayName, string Email);
public record GoogleAuthRequest(string IdToken);
public record SocialAuthResponse(string Token, string UserId, string Username, string Email);
public record LoginResponse(string Token, object User);
public record RegisterResponse(Guid Id, string Username);
