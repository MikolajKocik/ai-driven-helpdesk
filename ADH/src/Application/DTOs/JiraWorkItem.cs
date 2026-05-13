namespace Application.DTOs;

public sealed class JiraWorkItem
{
    public Guid Id = Guid.NewGuid();
    public string Summary { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid UserId { get; private set; }
}


