namespace TaskFlow.Modules.WorkItems.Application.Responses;

/// <summary>
/// Represents pagination metadata for paginated list responses.
/// </summary>
/// <param name="CurrentPage">The current page number (1-based).</param>
/// <param name="PageSize">The number of items per page.</param>
/// <param name="TotalItems">The total number of items across all pages.</param>
/// <param name="TotalPages">The total number of pages.</param>
public sealed record PaginationMetadataDto(
    int CurrentPage,
    int PageSize,
    int TotalItems,
    int TotalPages
);
