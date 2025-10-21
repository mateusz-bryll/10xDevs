using TaskFlow.Modules.WorkItems.Domain.ValueObjects;

namespace TaskFlow.Modules.WorkItems.Application.Responses;

/// <summary>
/// Represents a simplified project for list views (without timestamps).
/// Maps from: Domain.Entities.Project (subset of fields)
/// Used in: GET /api/projects (paginated list)
///
/// Note: ID is exposed as strongly typed (ProjectId) per semantic-ids.md conventions.
/// JSON serialization automatically converts it to Guid via StronglyTypedId converter.
/// </summary>
/// <param name="Id">The unique identifier of the project.</param>
/// <param name="Name">The project name.</param>
/// <param name="Description">Optional project description.</param>
public sealed record ProjectListItemDto(
    ProjectId Id,
    string Name,
    string? Description
);
