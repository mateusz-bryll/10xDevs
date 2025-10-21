using TaskFlow.Modules.Users;
using TaskFlow.Modules.WorkItems.Domain.ValueObjects;

namespace TaskFlow.Modules.WorkItems.Application.Responses;

/// <summary>
/// Represents a simplified work item for list views (without timestamps, description, or progress).
/// Maps from: Domain.Entities.WorkItem (subset of fields)
/// Used in: GET /api/projects/{projectId}/work-items (paginated list with lazy loading support)
///
/// Note: IDs are exposed as strongly typed (WorkItemId, ProjectId, UserId) per semantic-ids.md conventions.
/// JSON serialization automatically converts them to primitives (Guid/string) via StronglyTypedId converters.
/// Enums (WorkItemType, Status) are exposed as strings for JSON serialization.
/// </summary>
/// <param name="Id">The unique identifier of the work item.</param>
/// <param name="ProjectId">The project ID this work item belongs to.</param>
/// <param name="ParentId">The parent work item ID, or null for Epic-level items.</param>
/// <param name="WorkItemType">The work item type: "Epic", "Story", or "Task" (from WorkItemType enum).</param>
/// <param name="Title">The work item title.</param>
/// <param name="Status">The current status: "New", "Ready", "InProgress", or "Done" (from WorkItemStatus enum).</param>
/// <param name="AssignedUserId">The assigned user's ID (Auth0 format), or null if unassigned.</param>
/// <param name="HasChildren">Indicates whether this work item has child work items (used for lazy loading in tree view).</param>
public sealed record WorkItemListItemDto(
    WorkItemId Id,
    ProjectId ProjectId,
    WorkItemId? ParentId,
    string WorkItemType,
    string Title,
    string Status,
    UserId? AssignedUserId,
    bool HasChildren
);
