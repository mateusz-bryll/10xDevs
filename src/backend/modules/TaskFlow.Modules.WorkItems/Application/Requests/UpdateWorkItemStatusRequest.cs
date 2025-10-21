namespace TaskFlow.Modules.WorkItems.Application.Requests;

/// <summary>
/// Request payload for updating a work item's status.
/// Maps to: Domain.Entities.WorkItem.Status property
/// Used in: PUT /api/projects/{projectId}/work-items/{workItemId}/status
/// Validation: UpdateWorkItemStatusRequestValidator (FluentValidation)
///
/// Per PRD: All status transitions are allowed without restrictions.
/// </summary>
public sealed record UpdateWorkItemStatusRequest
{
    /// <summary>
    /// The new status: "New", "Ready", "InProgress", or "Done" (required, case-insensitive).
    /// All status transitions are allowed.
    /// </summary>
    public string? Status { get; init; }
}
