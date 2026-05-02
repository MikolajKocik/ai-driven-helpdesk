using System;
using System.Collections.Generic;

namespace ADH.Core.Entities;

public sealed class Asset
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    
    public Guid AssetTypeId { get; set; }
    public AssetType? AssetType { get; set; }
    
    public Guid? UserId { get; set; }
    public AppUser? User { get; set; }
    
    public string Status { get; set; } = "Active"; // Active, Maintenance, Retired
    public DateTime LastAuditDate { get; set; } = DateTime.UtcNow;
    
    public ICollection<Ticket> RelatedTickets { get; set; } = new List<Ticket>();
}

public sealed class AssetType
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty; // e.g. Laptop, Server, License, Mobile
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
