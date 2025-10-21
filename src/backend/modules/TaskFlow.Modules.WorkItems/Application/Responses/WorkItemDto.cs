using TaskFlow.Modules.Users;
using TaskFlow.Modules.WorkItems.Domain.ValueObjects;

namespace TaskFlow.Modules.WorkItems.Application.Responses;

/// <summary>
/// Represents a work item with full details including progress and child information.
/// Maps from: Domain.Entities.WorkItem with calculated progress
/// Used in: GET /api/projects/{projectId}/work-items/{workItemId},
///          POST /api/projects/{projectId}/work-items,
///          PUT /api/projects/{projectId}/work-items/{workItemId},
///          PUT /api/projects/{projectId}/work-items/{workItemId}/status,
///          PUT /api/projects/{projectId}/work-items/{workItemId}/assign
///
/// Note: IDs are exposed as strongly typed (WorkItemId, ProjectId, UserId) per semantic-ids.md conventions.
/// JSON serialization automatically converts them to primitives (Guid/string) via StronglyTypedId converters.
/// Enums (WorkItemType, Status) are exposed as strings for JSON serialization.
/// </summary>
/// <param name="Id">The unique identifier of the work item.</param>
/// <param name="ProjectId">The project ID this work item belongs to.</param>
/// <param name="ParentId">The parent work item ID, or null for Epic-level items.</param>
/// <param name="WorkItemType">The work item type: "Epic", "Story", or "Task" (from WorkItemType enum).</param>
/// <param name="Title">The work item title (max 200 characters).</param>
/// <param name="Description">Optional work item description (max 5000 characters).</param>
/// <param name="Status">The current status: "New", "Ready", "InProgress", or "Done" (from WorkItemStatus enum).</param>
/// <param name="AssignedUserId">The assigned user's ID (Auth0 format), or null if unassigned.</param>
/// <param name="CreatedAt">The timestamp when the work item was created.</param>
/// <param name="UpdatedAt">The timestamp when the work item was last updated.</param>
/// <param name="Progress">Progress information calculated from direct children (completed/total).</param>
/// <param name="HasChildren">Indicates whether this work item has child work items.</param>
public sealed record WorkItemDto(
    WorkItemId Id,
    ProjectId ProjectId,
    WorkItemId? ParentId,
    string WorkItemType,
    string Title,
    string? Description,
    string Status,
    UserId? AssignedUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    ProgressDto Progress,
    bool HasChildren
);
