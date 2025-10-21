using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskFlow.Modules.Users;
using TaskFlow.Modules.WorkItems.Application.Requests;
using TaskFlow.Modules.WorkItems.Application.Responses;
using TaskFlow.Modules.WorkItems.Domain.Entities;
using TaskFlow.Modules.WorkItems.Domain.Enums;
using TaskFlow.Modules.WorkItems.Domain.ValueObjects;
using TaskFlow.Modules.WorkItems.Infrastructure.Persistence;

namespace TaskFlow.Modules.WorkItems.Application.Services;

/// <summary>
/// Service for managing WorkItem entities with business logic enforcement.
/// Handles CRUD operations, hierarchy validation, progress calculation, and cascade deletion.
/// </summary>
public interface IWorkItemsService
{
    /// <summary>
    /// Lists work items for a project with optional parent filtering and pagination.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="parentId">Optional parent ID filter (null for root Epics).</param>
    /// <param name="page">Page number (default: 1, min: 1).</param>
    /// <param name="pageSize">Items per page (default: 20, min: 1, max: 100).</param>
    /// <param name="userId">The requesting user's ID (must own the project).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of work items.</returns>
    /// <exception cref="UnauthorizedAccessException">User does not own the project.</exception>
    /// <exception cref="KeyNotFoundException">Project not found.</exception>
    Task<WorkItemListResponse> ListWorkItemsAsync(
        ProjectId projectId,
        WorkItemId? parentId,
        int page = 1,
        int pageSize = 20,
        UserId? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific work item by ID with calculated progress. Verifies project ownership.
    /// </summary>
    /// <param name="workItemId">The work item ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="userId">The requesting user's ID (must own the project).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Work item details with progress.</returns>
    /// <exception cref="UnauthorizedAccessException">User does not own the project.</exception>
    /// <exception cref="KeyNotFoundException">Work item or project not found.</exception>
    Task<WorkItemDto> GetWorkItemByIdAsync(
        WorkItemId workItemId,
        ProjectId projectId,
        UserId userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new work item with hierarchy validation.
    /// </summary>
    /// <param name="request">Work item creation data.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="userId">The requesting user's ID (must own the project).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created work item details.</returns>
    /// <exception cref="UnauthorizedAccessException">User does not own the project.</exception>
    /// <exception cref="ArgumentException">Hierarchy validation failed.</exception>
    /// <exception cref="KeyNotFoundException">Project or parent work item not found.</exception>
    Task<WorkItemDto> CreateWorkItemAsync(
        CreateWorkItemRequest request,
        ProjectId projectId,
        UserId userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing work item. Validates hierarchy if parentId changed.
    /// </summary>
    /// <param name="workItemId">The work item ID to update.</param>
    /// <param name="request">Updated work item data.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="userId">The requesting user's ID (must own the project).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated work item details.</returns>
    /// <exception cref="UnauthorizedAccessException">User does not own the project.</exception>
    /// <exception cref="ArgumentException">Hierarchy validation failed.</exception>
    /// <exception cref="KeyNotFoundException">Work item, project, or parent not found.</exception>
    Task<WorkItemDto> UpdateWorkItemAsync(
        WorkItemId workItemId,
        UpdateWorkItemRequest request,
        ProjectId projectId,
        UserId userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a work item and all its children (cascade). Verifies project ownership.
    /// </summary>
    /// <param name="workItemId">The work item ID to delete.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="userId">The requesting user's ID (must own the project).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deletion summary with total count of deleted items.</returns>
    /// <exception cref="UnauthorizedAccessException">User does not own the project.</exception>
    /// <exception cref="KeyNotFoundException">Work item or project not found.</exception>
    Task<DeleteWorkItemResponse> DeleteWorkItemAsync(
        WorkItemId workItemId,
        ProjectId projectId,
        UserId userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the status of a work item. All transitions allowed.
    /// </summary>
    /// <param name="workItemId">The work item ID.</param>
    /// <param name="request">Status update data.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="userId">The requesting user's ID (must own the project).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated work item details.</returns>
    /// <exception cref="UnauthorizedAccessException">User does not own the project.</exception>
    /// <exception cref="KeyNotFoundException">Work item or project not found.</exception>
    Task<WorkItemDto> UpdateWorkItemStatusAsync(
        WorkItemId workItemId,
        UpdateWorkItemStatusRequest request,
        ProjectId projectId,
        UserId userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a work item to a user or unassigns if userId is null.
    /// </summary>
    /// <param name="workItemId">The work item ID.</param>
    /// <param name="request">Assignment data.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="userId">The requesting user's ID (must own the project).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated work item details.</returns>
    /// <exception cref="UnauthorizedAccessException">User does not own the project.</exception>
    /// <exception cref="KeyNotFoundException">Work item, project, or assigned user not found.</exception>
    Task<WorkItemDto> AssignWorkItemAsync(
        WorkItemId workItemId,
        AssignWorkItemRequest request,
        ProjectId projectId,
        UserId userId,
        CancellationToken cancellationToken = default);
}

internal sealed class WorkItemsService(
    WorkItemsDatabaseContext context,
    IUsersService usersService,
    ILogger<WorkItemsService> logger) : IWorkItemsService
{
    public async Task<WorkItemListResponse> ListWorkItemsAsync(
        ProjectId projectId,
        WorkItemId? parentId,
        int page = 1,
        int pageSize = 20,
        UserId? userId = null,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Listing work items for project {ProjectId} with parent {ParentId} (page {Page}, pageSize {PageSize})",
            projectId, parentId, page, pageSize);

        // Verify project exists and user owns it
        if (userId is not null)
        {
            await VerifyProjectOwnershipAsync(projectId, userId.Value, cancellationToken);
        }

        // Validate pagination parameters
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = context.WorkItems
            .Where(w => w.ProjectId == projectId && w.ParentId == parentId)
            .OrderBy(w => w.CreatedAt);

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var workItems = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(w => w.Children)
            .ToListAsync(cancellationToken);

        var workItemDtos = workItems.Select(w => new WorkItemListItemDto(
            w.Id,
            w.ProjectId,
            w.ParentId,
            w.WorkItemType.ToString(),
            w.Title,
            w.Status.ToString(),
            w.AssignedUserId,
            w.Children.Any())).ToList();

        return new WorkItemListResponse(
            workItemDtos,
            new PaginationMetadataDto(page, pageSize, totalItems, totalPages));
    }

    public async Task<WorkItemDto> GetWorkItemByIdAsync(
        WorkItemId workItemId,
        ProjectId projectId,
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting work item {WorkItemId} for project {ProjectId}",
            workItemId, projectId);

        // Verify project ownership
        await VerifyProjectOwnershipAsync(projectId, userId, cancellationToken);

        var workItem = await context.WorkItems
            .Include(w => w.Children)
            .FirstOrDefaultAsync(w => w.Id == workItemId && w.ProjectId == projectId, cancellationToken);

        if (workItem is null)
        {
            logger.LogWarning("Work item {WorkItemId} not found in project {ProjectId}",
                workItemId, projectId);
            throw new KeyNotFoundException($"Work item with ID '{workItemId}' was not found");
        }

        return await MapToDtoAsync(workItem, cancellationToken);
    }

    public async Task<WorkItemDto> CreateWorkItemAsync(
        CreateWorkItemRequest request,
        ProjectId projectId,
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating work item '{Title}' of type {Type} in project {ProjectId}",
            request.Title, request.WorkItemType, projectId);

        // Verify project ownership
        await VerifyProjectOwnershipAsync(projectId, userId, cancellationToken);

        // Parse and validate work item type
        if (!Enum.TryParse<WorkItemType>(request.WorkItemType, ignoreCase: true, out var workItemType))
        {
            throw new ArgumentException(
                $"Invalid work item type '{request.WorkItemType}'. Must be Epic, Story, or Task.",
                nameof(request.WorkItemType));
        }

        // Validate hierarchy rules
        await ValidateHierarchyAsync(workItemType, request.ParentId, projectId, cancellationToken);

        // Verify assigned user exists if provided
        if (request.AssignedUserId is not null)
        {
            await VerifyUserExistsAsync(request.AssignedUserId.Value, cancellationToken);
        }

        var workItem = new WorkItem(
            WorkItemId.New(),
            projectId,
            request.ParentId,
            workItemType,
            request.Title,
            request.Description,
            request.AssignedUserId);

        context.WorkItems.Add(workItem);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Work item {WorkItemId} created successfully", workItem.Id);

        return await MapToDtoAsync(workItem, cancellationToken);
    }

    public async Task<WorkItemDto> UpdateWorkItemAsync(
        WorkItemId workItemId,
        UpdateWorkItemRequest request,
        ProjectId projectId,
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating work item {WorkItemId} in project {ProjectId}",
            workItemId, projectId);

        // Verify project ownership
        await VerifyProjectOwnershipAsync(projectId, userId, cancellationToken);

        var workItem = await context.WorkItems
            .Include(w => w.Children)
            .FirstOrDefaultAsync(w => w.Id == workItemId && w.ProjectId == projectId, cancellationToken);

        if (workItem is null)
        {
            logger.LogWarning("Work item {WorkItemId} not found in project {ProjectId}",
                workItemId, projectId);
            throw new KeyNotFoundException($"Work item with ID '{workItemId}' was not found");
        }

        // Validate hierarchy if parentId changed
        if (request.ParentId != workItem.ParentId)
        {
            await ValidateHierarchyAsync(workItem.WorkItemType, request.ParentId, projectId, cancellationToken);
        }

        // TODO: Add Update method to WorkItem entity for proper encapsulation
        // For MVP, using EF Core's change tracking with Entry API
        context.Entry(workItem).Property(w => w.Title).CurrentValue = request.Title;
        context.Entry(workItem).Property(w => w.Description).CurrentValue = request.Description;
        context.Entry(workItem).Property(w => w.ParentId).CurrentValue = request.ParentId;
        context.Entry(workItem).Property(w => w.UpdatedAt).CurrentValue = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Work item {WorkItemId} updated successfully", workItemId);

        return await MapToDtoAsync(workItem, cancellationToken);
    }

    public async Task<DeleteWorkItemResponse> DeleteWorkItemAsync(
        WorkItemId workItemId,
        ProjectId projectId,
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Deleting work item {WorkItemId} from project {ProjectId}",
            workItemId, projectId);

        // Verify project ownership
        await VerifyProjectOwnershipAsync(projectId, userId, cancellationToken);

        var workItem = await context.WorkItems
            .Include(w => w.Children)
            .FirstOrDefaultAsync(w => w.Id == workItemId && w.ProjectId == projectId, cancellationToken);

        if (workItem is null)
        {
            logger.LogWarning("Work item {WorkItemId} not found in project {ProjectId}",
                workItemId, projectId);
            throw new KeyNotFoundException($"Work item with ID '{workItemId}' was not found");
        }

        // Count descendants for response (cascade delete handled by EF Core configuration)
        var deletedCount = await CountDescendantsAsync(workItem, cancellationToken) + 1; // +1 for the item itself

        context.WorkItems.Remove(workItem);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Work item {WorkItemId} and {DeletedCount} descendants deleted successfully",
            workItemId, deletedCount - 1);

        return new DeleteWorkItemResponse(
            "Work item deleted successfully",
            deletedCount);
    }

    public async Task<WorkItemDto> UpdateWorkItemStatusAsync(
        WorkItemId workItemId,
        UpdateWorkItemStatusRequest request,
        ProjectId projectId,
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating status of work item {WorkItemId} to {Status}",
            workItemId, request.Status);

        // Verify project ownership
        await VerifyProjectOwnershipAsync(projectId, userId, cancellationToken);

        var workItem = await context.WorkItems
            .Include(w => w.Children)
            .FirstOrDefaultAsync(w => w.Id == workItemId && w.ProjectId == projectId, cancellationToken);

        if (workItem is null)
        {
            logger.LogWarning("Work item {WorkItemId} not found in project {ProjectId}",
                workItemId, projectId);
            throw new KeyNotFoundException($"Work item with ID '{workItemId}' was not found");
        }

        // Parse status
        if (!Enum.TryParse<WorkItemStatus>(request.Status, ignoreCase: true, out var status))
        {
            throw new ArgumentException(
                $"Invalid status '{request.Status}'. Must be New, Ready, InProgress, or Done.",
                nameof(request.Status));
        }

        // Update status using domain method
        workItem.UpdateStatus(status);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Work item {WorkItemId} status updated to {Status}",
            workItemId, status);

        return await MapToDtoAsync(workItem, cancellationToken);
    }

    public async Task<WorkItemDto> AssignWorkItemAsync(
        WorkItemId workItemId,
        AssignWorkItemRequest request,
        ProjectId projectId,
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Assigning work item {WorkItemId} to user {AssignedUserId}",
            workItemId, request.UserId);

        // Verify project ownership
        await VerifyProjectOwnershipAsync(projectId, userId, cancellationToken);

        var workItem = await context.WorkItems
            .Include(w => w.Children)
            .FirstOrDefaultAsync(w => w.Id == workItemId && w.ProjectId == projectId, cancellationToken);

        if (workItem is null)
        {
            logger.LogWarning("Work item {WorkItemId} not found in project {ProjectId}",
                workItemId, projectId);
            throw new KeyNotFoundException($"Work item with ID '{workItemId}' was not found");
        }

        // Verify assigned user exists if provided (null = unassign)
        if (request.UserId is not null)
        {
            await VerifyUserExistsAsync(request.UserId.Value, cancellationToken);
            workItem.Assign(request.UserId.Value);
        }
        else
        {
            // Unassign by setting to null
            context.Entry(workItem).Property(w => w.AssignedUserId).CurrentValue = null;
            context.Entry(workItem).Property(w => w.UpdatedAt).CurrentValue = DateTimeOffset.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Work item {WorkItemId} assigned successfully", workItemId);

        return await MapToDtoAsync(workItem, cancellationToken);
    }

    #region Private Helper Methods

    /// <summary>
    /// Verifies that the user owns the specified project.
    /// </summary>
    private async Task VerifyProjectOwnershipAsync(
        ProjectId projectId,
        UserId userId,
        CancellationToken cancellationToken)
    {
        var project = await context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project is null)
        {
            logger.LogWarning("Project {ProjectId} not found", projectId);
            throw new KeyNotFoundException($"Project with ID '{projectId}' was not found");
        }

        if (project.OwnerId != userId)
        {
            logger.LogWarning("User {UserId} attempted to access project {ProjectId} owned by {OwnerId}",
                userId, projectId, project.OwnerId);
            throw new UnauthorizedAccessException("You do not have permission to access this project");
        }
    }

    /// <summary>
    /// Validates work item hierarchy rules based on type and parent.
    /// Epic: parentId must be null
    /// Story: parentId must reference an existing Epic
    /// Task: parentId must reference an existing Story
    /// </summary>
    private async Task ValidateHierarchyAsync(
        WorkItemType workItemType,
        WorkItemId? parentId,
        ProjectId projectId,
        CancellationToken cancellationToken)
    {
        switch (workItemType)
        {
            case WorkItemType.Epic:
                if (parentId is not null)
                {
                    logger.LogWarning("Attempted to create Epic with parent {ParentId}", parentId);
                    throw new ArgumentException("Epic cannot have a parent", nameof(parentId));
                }
                break;

            case WorkItemType.Story:
                if (parentId is null)
                {
                    logger.LogWarning("Attempted to create Story without parent");
                    throw new ArgumentException("Story must have an Epic as parent", nameof(parentId));
                }

                var epicParent = await context.WorkItems
                    .FirstOrDefaultAsync(w => w.Id == parentId && w.ProjectId == projectId, cancellationToken);

                if (epicParent is null)
                {
                    logger.LogWarning("Parent work item {ParentId} not found", parentId);
                    throw new KeyNotFoundException($"Parent work item with ID '{parentId}' was not found");
                }

                if (epicParent.WorkItemType != WorkItemType.Epic)
                {
                    logger.LogWarning("Story parent {ParentId} is not an Epic (type: {Type})",
                        parentId, epicParent.WorkItemType);
                    throw new ArgumentException("Story must have an Epic as parent", nameof(parentId));
                }
                break;

            case WorkItemType.Task:
                if (parentId is null)
                {
                    logger.LogWarning("Attempted to create Task without parent");
                    throw new ArgumentException("Task must have a Story as parent", nameof(parentId));
                }

                var storyParent = await context.WorkItems
                    .FirstOrDefaultAsync(w => w.Id == parentId && w.ProjectId == projectId, cancellationToken);

                if (storyParent is null)
                {
                    logger.LogWarning("Parent work item {ParentId} not found", parentId);
                    throw new KeyNotFoundException($"Parent work item with ID '{parentId}' was not found");
                }

                if (storyParent.WorkItemType != WorkItemType.Story)
                {
                    logger.LogWarning("Task parent {ParentId} is not a Story (type: {Type})",
                        parentId, storyParent.WorkItemType);
                    throw new ArgumentException("Task must have a Story as parent", nameof(parentId));
                }
                break;
        }
    }

    /// <summary>
    /// Verifies that a user exists by fetching from the users service.
    /// </summary>
    private async Task VerifyUserExistsAsync(UserId userId, CancellationToken cancellationToken)
    {
        try
        {
            await usersService.GetUsersByIdAsync(userId, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to verify user {UserId} exists", userId);
            throw new KeyNotFoundException($"User with ID '{userId}' was not found", ex);
        }
    }

    /// <summary>
    /// Recursively counts all descendants of a work item.
    /// </summary>
    private async Task<int> CountDescendantsAsync(WorkItem workItem, CancellationToken cancellationToken)
    {
        var count = 0;

        // Load children if not already loaded
        if (!context.Entry(workItem).Collection(w => w.Children).IsLoaded)
        {
            await context.Entry(workItem).Collection(w => w.Children).LoadAsync(cancellationToken);
        }

        foreach (var child in workItem.Children)
        {
            count++; // Count the child
            count += await CountDescendantsAsync(child, cancellationToken); // Count child's descendants
        }

        return count;
    }

    /// <summary>
    /// Calculates progress for a work item based on direct children.
    /// For leaf items: 0/0/0
    /// For parent items: completed/total/percentage
    /// </summary>
    private static ProgressDto CalculateProgress(WorkItem workItem)
    {
        var childrenCount = workItem.Children.Count;

        if (childrenCount == 0)
        {
            // Leaf item - no progress
            return new ProgressDto(0, 0, 0);
        }

        var completedCount = workItem.Children.Count(c => c.Status == WorkItemStatus.Done);
        var percentage = (int)Math.Round((double)completedCount / childrenCount * 100);

        return new ProgressDto(completedCount, childrenCount, percentage);
    }

    /// <summary>
    /// Maps WorkItem entity to WorkItemDto with calculated progress.
    /// </summary>
    private async Task<WorkItemDto> MapToDtoAsync(WorkItem workItem, CancellationToken cancellationToken)
    {
        // Ensure children are loaded for progress calculation
        if (!context.Entry(workItem).Collection(w => w.Children).IsLoaded)
        {
            await context.Entry(workItem).Collection(w => w.Children).LoadAsync(cancellationToken);
        }

        var progress = CalculateProgress(workItem);
        var hasChildren = workItem.Children.Any();

        return new WorkItemDto(
            workItem.Id,
            workItem.ProjectId,
            workItem.ParentId,
            workItem.WorkItemType.ToString(),
            workItem.Title,
            workItem.Description,
            workItem.Status.ToString(),
            workItem.AssignedUserId,
            workItem.CreatedAt,
            workItem.UpdatedAt,
            progress,
            hasChildren);
    }

    #endregion
}
