using System;

namespace ADH.Core.Entities;

public sealed class SlaPolicy
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty; // e.g. "Gold", "Silver"
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
    
    public int ResponseTimeMinutes { get; set; }
    public int ResolutionTimeMinutes { get; set; }
}
