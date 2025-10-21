using TaskFlow.Modules.Users;
using TaskFlow.Modules.WorkItems.Domain.ValueObjects;

namespace TaskFlow.Modules.WorkItems.Application.Responses;

/// <summary>
/// Represents a project with full details including timestamps.
/// Maps from: Domain.Entities.Project
/// Used in: GET /api/projects/{id}, POST /api/projects, PUT /api/projects/{id}
///
/// Note: IDs are exposed as strongly typed (ProjectId, UserId) per semantic-ids.md conventions.
/// JSON serialization automatically converts them to primitives (Guid/string) via StronglyTypedId converters.
/// </summary>
/// <param name="Id">The unique identifier of the project.</param>
/// <param name="Name">The project name (max 200 characters).</param>
/// <param name="Description">Optional project description (max 2000 characters).</param>
/// <param name="OwnerId">The owner's user ID (Auth0 format).</param>
/// <param name="CreatedAt">The timestamp when the project was created.</param>
/// <param name="UpdatedAt">The timestamp when the project was last updated.</param>
public sealed record ProjectDto(
    ProjectId Id,
    string Name,
    string? Description,
    UserId OwnerId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
