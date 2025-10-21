using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Modules.WorkItems.Application.Requests;

/// <summary>
/// Request payload for updating an existing project.
/// Maps to: Domain.Entities.Project.Name and Description properties
/// Used in: PUT /api/projects/{id}
/// </summary>
public sealed record UpdateProjectRequest
{
    /// <summary>
    /// The updated project name (required, max 200 characters).
    /// </summary>
    [Required(ErrorMessage = "Project name is required.")]
    [MaxLength(200, ErrorMessage = "Project name cannot exceed 200 characters.")]
    public required string Name { get; init; }

    /// <summary>
    /// The updated project description (optional, max 2000 characters).
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Project description cannot exceed 2000 characters.")]
    public string? Description { get; init; }
}
