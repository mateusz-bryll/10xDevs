using TaskFlow.Modules.Users;

namespace TaskFlow.Modules.WorkItems.Application.Requests;

/// <summary>
/// Request payload for assigning or unassigning a work item to/from a user.
/// Maps to: Domain.Entities.WorkItem.AssignedUserId property
/// Used in: PUT /api/projects/{projectId}/work-items/{workItemId}/assign
///
/// Note: UserId is exposed as strongly typed per semantic-ids.md conventions.
/// ASP.NET Core model binding automatically converts from JSON string (Auth0 format) via StronglyTypedId converter.
/// </summary>
public sealed record AssignWorkItemRequest
{
    /// <summary>
    /// The user ID to assign the work item to (Auth0 format), or null to unassign.
    /// Per MVP: Users can assign work items to themselves or any other user (no restrictions).
    /// </summary>
    public UserId? UserId { get; init; }
}
