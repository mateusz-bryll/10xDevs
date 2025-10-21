using TaskFlow.Modules.WorkItems.Domain.ValueObjects;

namespace TaskFlow.Modules.WorkItems.Application.Requests;

/// <summary>
/// Request payload for updating an existing work item (title, description, and parent).
/// Maps to: Domain.Entities.WorkItem properties (Title, Description, ParentId)
/// Used in: PUT /api/projects/{projectId}/work-items/{workItemId}
/// Validation: UpdateWorkItemRequestValidator (FluentValidation)
///
/// Note: WorkItemType cannot be changed via update (not included in this request).
/// Changing ParentId must maintain hierarchy rules (Epic → Story → Task).
/// IDs are exposed as strongly typed (WorkItemId) per semantic-ids.md conventions.
/// ASP.NET Core model binding automatically converts from JSON primitives via StronglyTypedId converters.
/// </summary>
public sealed record UpdateWorkItemRequest
{
    /// <summary>
    /// The updated work item title (required, max 200 characters).
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// The updated work item description (optional, max 5000 characters).
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The updated parent work item ID. Must maintain hierarchy rules based on the work item's type.
    /// </summary>
    public WorkItemId? ParentId { get; init; }
}
