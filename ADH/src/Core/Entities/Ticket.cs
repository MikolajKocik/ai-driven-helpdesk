using System;
using ADH.Core.Attributes;

namespace ADH.Core.Entities;

[Auditable]
public class Ticket
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Description { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Open"; 
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
    public bool IsResolved => Status == "Resolved" || Status == "Closed";

    public DateTime? SlaResponseDeadline { get; set; }
    public DateTime? SlaResolutionDeadline { get; set; }
    public string SlaStatus { get; set; } = "InThreshold"; // InThreshold, Violated, ApproachingThreshold

    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    // Production Integration Fields
    public string? ExternalSystem { get; set; } 
    public string? ExternalId { get; set; }     
    public DateTime? LastSyncAt { get; set; }
    
    public Guid? AssetId { get; set; }
    public Asset? Asset { get; set; }
}
