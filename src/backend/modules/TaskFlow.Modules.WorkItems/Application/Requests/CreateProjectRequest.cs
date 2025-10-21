using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Modules.WorkItems.Application.Requests;

/// <summary>
/// Request payload for creating a new project.
/// Maps to: Domain.Entities.Project constructor parameters
/// Used in: POST /api/projects
/// </summary>
public sealed record CreateProjectRequest
{
    /// <summary>
    /// The project name (required, max 200 characters).
    /// </summary>
    [Required(ErrorMessage = "Project name is required.")]
    [MaxLength(200, ErrorMessage = "Project name cannot exceed 200 characters.")]
    public required string Name { get; init; }

    /// <summary>
    /// Optional project description (max 2000 characters).
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Project description cannot exceed 2000 characters.")]
    public string? Description { get; init; }
}
