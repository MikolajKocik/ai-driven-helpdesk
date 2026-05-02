namespace ADH.Application.DTOs;

public record LoginRequest(string Username, string Password);
public record RegisterRequest(string Username, string Password, string DisplayName, string Email);
