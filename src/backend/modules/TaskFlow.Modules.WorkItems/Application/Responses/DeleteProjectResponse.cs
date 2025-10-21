namespace TaskFlow.Modules.WorkItems.Application.Responses;

/// <summary>
/// Response payload for project deletion confirmation.
/// Used in: DELETE /api/projects/{id}
/// </summary>
/// <param name="Message">Confirmation message.</param>
/// <param name="DeletedCount">Total number of work items deleted (including all child items due to cascade delete).</param>
public sealed record DeleteProjectResponse(
    string Message,
    int DeletedCount
);
