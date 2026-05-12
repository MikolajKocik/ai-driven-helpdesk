using ADH.Core.Attributes;

namespace ADH.Core.Entities;

[Auditable]
public sealed class Ticket
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Description { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public string Status { get; private set; } = "Open"; 
    public string Priority { get; private set; } = "Medium"; // Low, Medium, High, Critical
    public bool IsResolved => Status == "Resolved" || Status == "Closed";

    public DateTime? SlaResponseDeadline { get; private set; }
    public DateTime? SlaResolutionDeadline { get; private set; }
    public string SlaStatus { get; private set; } = "InThreshold"; // InThreshold, Violated, ApproachingThreshold

    public Guid UserId { get; init; }
    public AppUser User { get; init; } = null!;

    public string? ExternalSystem { get; private set; } 
    public string? ExternalId { get; private set; }     
    public DateTime? LastSyncAt { get; private set; }
    
    public Guid? AssetId { get; init; }
    public Asset? Asset { get; init; }

    private Ticket() {}

    public Ticket(string summary, string description, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(summary))
            throw new ArgumentException("Summary cannot be empty");

        Summary = summary;
        Description = description;
        UserId = userId;
    }

    public void LinkToExternalSystem(string externalSystem, string externalId)
    {
        if (string.IsNullOrEmpty(externalId)) 
            throw new ArgumentException("External ID is required.");
        
        ExternalSystem = externalSystem;
        ExternalId = externalId;
        LastSyncAt = DateTime.UtcNow;
    }

    public void UpdateStatusFromWebhook(string newStatus)
    {
        Status = newStatus;
        LastSyncAt = DateTime.UtcNow;
    }
}
