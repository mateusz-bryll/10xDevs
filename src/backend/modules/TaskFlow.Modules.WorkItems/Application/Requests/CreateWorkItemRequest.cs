using TaskFlow.Modules.Users;
using TaskFlow.Modules.WorkItems.Domain.ValueObjects;

namespace TaskFlow.Modules.WorkItems.Application.Requests;

/// <summary>
/// Request payload for creating a new work item.
/// Maps to: Domain.Entities.WorkItem constructor parameters
/// Used in: POST /api/projects/{projectId}/work-items
/// Validation: CreateWorkItemRequestValidator (FluentValidation)
///
/// Hierarchy Rules (enforced via validation):
/// - Epic: ParentId must be null
/// - Story: ParentId must reference an existing Epic
/// - Task: ParentId must reference an existing Story
///
/// Note: IDs are exposed as strongly typed (WorkItemId, UserId) per semantic-ids.md conventions.
/// ASP.NET Core model binding automatically converts from JSON primitives via StronglyTypedId converters.
/// </summary>
public sealed record CreateWorkItemRequest
{
    /// <summary>
    /// The parent work item ID. Must be null for Epic, Epic ID for Story, Story ID for Task.
    /// </summary>
    public WorkItemId? ParentId { get; init; }

    /// <summary>
    /// The work item type: "Epic", "Story", or "Task" (required, case-insensitive).
    /// </summary>
    public string? WorkItemType { get; init; }

    /// <summary>
    /// The work item title (required, max 200 characters).
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Optional work item description (max 5000 characters).
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The user ID to assign this work item to (Auth0 format), or null to leave unassigned.
    /// </summary>
    public UserId? AssignedUserId { get; init; }
}
