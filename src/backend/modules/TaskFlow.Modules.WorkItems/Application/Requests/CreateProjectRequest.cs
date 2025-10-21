namespace TaskFlow.Modules.WorkItems.Application.Requests;

/// <summary>
/// Request payload for creating a new project.
/// Maps to: Domain.Entities.Project constructor parameters
/// Used in: POST /api/projects
/// Validation: CreateProjectRequestValidator (FluentValidation)
/// </summary>
public sealed record CreateProjectRequest
{
    /// <summary>
    /// The project name (required, max 200 characters).
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Optional project description (max 2000 characters).
    /// </summary>
    public string? Description { get; init; }
}
