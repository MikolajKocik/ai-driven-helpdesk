using System;

namespace ADH.Core.Entities;

public class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    
    // RBAC and Metadata
    public string Role { get; set; } = "Client"; // Admin, Agent, Client
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
