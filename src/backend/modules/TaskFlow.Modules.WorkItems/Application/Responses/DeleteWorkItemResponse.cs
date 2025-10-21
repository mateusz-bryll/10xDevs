namespace TaskFlow.Modules.WorkItems.Application.Responses;

/// <summary>
/// Response payload for work item deletion confirmation.
/// Used in: DELETE /api/projects/{projectId}/work-items/{workItemId}
///
/// Cascade Delete Behavior:
/// - Deleting an Epic deletes all Stories and Tasks beneath it
/// - Deleting a Story deletes all Tasks beneath it
/// - Deleting a Task deletes only the Task (no children)
/// </summary>
/// <param name="Message">Confirmation message.</param>
/// <param name="DeletedCount">Total number of work items deleted (including the parent and all children due to cascade delete).</param>
public sealed record DeleteWorkItemResponse(
    string Message,
    int DeletedCount
);
