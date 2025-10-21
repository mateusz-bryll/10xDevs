namespace TaskFlow.Modules.WorkItems.Application.Responses;

/// <summary>
/// Response wrapper for paginated project list.
/// Used in: GET /api/projects
/// </summary>
/// <param name="Projects">List of projects for the current page.</param>
/// <param name="Pagination">Pagination metadata.</param>
public sealed record ProjectListResponse(
    List<ProjectListItemDto> Projects,
    PaginationMetadataDto Pagination
);
