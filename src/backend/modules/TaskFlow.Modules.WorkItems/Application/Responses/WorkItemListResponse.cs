namespace TaskFlow.Modules.WorkItems.Application.Responses;

/// <summary>
/// Response wrapper for paginated work item list with lazy loading support.
/// Used in: GET /api/projects/{projectId}/work-items
///
/// Supports lazy loading for tree view:
/// - When parentId is not specified or null, returns top-level Epics
/// - When parentId is an Epic ID, returns Stories under that Epic
/// - When parentId is a Story ID, returns Tasks under that Story
/// </summary>
/// <param name="WorkItems">List of work items for the current page and parent filter.</param>
/// <param name="Pagination">Pagination metadata.</param>
public sealed record WorkItemListResponse(
    List<WorkItemListItemDto> WorkItems,
    PaginationMetadataDto Pagination
);
