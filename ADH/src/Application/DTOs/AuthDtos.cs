namespace ADH.Application.DTOs;

public record LoginRequest(string Username, string Password);
public record RegisterRequest(string Username, string Password, string DisplayName, string Email);
public record GoogleAuthRequest(string IdToken);
public record MicrosoftAuthRequest(string IdToken);
public record SocialAuthResponse(string Token, string UserId, string Username, string Email);
