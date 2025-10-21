using TaskFlow.Modules.Users;
using TaskFlow.Modules.WorkItems.Domain.ValueObjects;

namespace TaskFlow.Modules.WorkItems.Domain.Entities;

public sealed class Project
{
    public ProjectId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public UserId OwnerId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // Navigation
    public ICollection<WorkItem> WorkItems { get; private set; } = new List<WorkItem>();

    private Project() { } // EF Core

    public Project(ProjectId id, string name, string? description, UserId ownerId)
    {
        Id = id;
        Name = name;
        Description = description;
        OwnerId = ownerId;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
