namespace TaskFlow.Modules.WorkItems.Application.Requests;

/// <summary>
/// Request payload for updating an existing project.
/// Maps to: Domain.Entities.Project.Name and Description properties
/// Used in: PUT /api/projects/{id}
/// Validation: UpdateProjectRequestValidator (FluentValidation)
/// </summary>
public sealed record UpdateProjectRequest
{
    /// <summary>
    /// The updated project name (required, max 200 characters).
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// The updated project description (optional, max 2000 characters).
    /// </summary>
    public string? Description { get; init; }
}
