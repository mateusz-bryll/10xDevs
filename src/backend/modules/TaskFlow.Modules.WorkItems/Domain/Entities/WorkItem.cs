using TaskFlow.Modules.Users;
using TaskFlow.Modules.WorkItems.Domain.Enums;
using TaskFlow.Modules.WorkItems.Domain.ValueObjects;

namespace TaskFlow.Modules.WorkItems.Domain.Entities;

public sealed class WorkItem
{
    public WorkItemId Id { get; private set; }
    public ProjectId ProjectId { get; private set; }
    public WorkItemId? ParentId { get; private set; }
    public WorkItemType WorkItemType { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public WorkItemStatus Status { get; private set; }
    public UserId? AssignedUserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // Navigation
    public Project Project { get; private set; } = null!;
    public WorkItem? Parent { get; private set; }
    public ICollection<WorkItem> Children { get; private set; } = new List<WorkItem>();

    private WorkItem() { } // EF Core

    public WorkItem(
        WorkItemId id,
        ProjectId projectId,
        WorkItemId? parentId,
        WorkItemType workItemType,
        string title,
        string? description,
        UserId? assignedUserId)
    {
        Id = id;
        ProjectId = projectId;
        ParentId = parentId;
        WorkItemType = workItemType;
        Title = title;
        Description = description;
        Status = WorkItemStatus.New;
        AssignedUserId = assignedUserId;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateStatus(WorkItemStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Assign(UserId userId)
    {
        AssignedUserId = userId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
