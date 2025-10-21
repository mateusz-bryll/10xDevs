using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskFlow.Modules.Users;
using TaskFlow.Modules.WorkItems.Application.Requests;
using TaskFlow.Modules.WorkItems.Application.Responses;
using TaskFlow.Modules.WorkItems.Domain.Entities;
using TaskFlow.Modules.WorkItems.Domain.ValueObjects;
using TaskFlow.Modules.WorkItems.Infrastructure.Persistence;

namespace TaskFlow.Modules.WorkItems.Application.Services;

/// <summary>
/// Service for managing Project entities with business logic enforcement.
/// Handles CRUD operations, ownership verification, and cascade deletion.
/// </summary>
public interface IProjectsService
{
    /// <summary>
    /// Lists all projects owned by the specified user with pagination.
    /// </summary>
    /// <param name="userId">The owner's user ID.</param>
    /// <param name="page">Page number (default: 1, min: 1).</param>
    /// <param name="pageSize">Items per page (default: 20, min: 1, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of projects.</returns>
    Task<ProjectListResponse> ListProjectsAsync(
        UserId userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific project by ID. Verifies user ownership.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="userId">The requesting user's ID (must be owner).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Project details.</returns>
    /// <exception cref="UnauthorizedAccessException">User does not own the project.</exception>
    /// <exception cref="KeyNotFoundException">Project not found.</exception>
    Task<ProjectDto> GetProjectByIdAsync(
        ProjectId projectId,
        UserId userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new project owned by the specified user.
    /// </summary>
    /// <param name="request">Project creation data.</param>
    /// <param name="userId">The owner's user ID (from authentication).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created project details.</returns>
    Task<ProjectDto> CreateProjectAsync(
        CreateProjectRequest request,
        UserId userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing project. Verifies user ownership.
    /// </summary>
    /// <param name="projectId">The project ID to update.</param>
    /// <param name="request">Updated project data.</param>
    /// <param name="userId">The requesting user's ID (must be owner).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated project details.</returns>
    /// <exception cref="UnauthorizedAccessException">User does not own the project.</exception>
    /// <exception cref="KeyNotFoundException">Project not found.</exception>
    Task<ProjectDto> UpdateProjectAsync(
        ProjectId projectId,
        UpdateProjectRequest request,
        UserId userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a project and all its work items (cascade). Verifies user ownership.
    /// </summary>
    /// <param name="projectId">The project ID to delete.</param>
    /// <param name="userId">The requesting user's ID (must be owner).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deletion summary with total count of deleted work items.</returns>
    /// <exception cref="UnauthorizedAccessException">User does not own the project.</exception>
    /// <exception cref="KeyNotFoundException">Project not found.</exception>
    Task<DeleteProjectResponse> DeleteProjectAsync(
        ProjectId projectId,
        UserId userId,
        CancellationToken cancellationToken = default);
}

internal sealed class ProjectsService(
    WorkItemsDatabaseContext context,
    ILogger<ProjectsService> logger) : IProjectsService
{
    public async Task<ProjectListResponse> ListProjectsAsync(
        UserId userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Listing projects for user {UserId} (page {Page}, pageSize {PageSize})", userId, page, pageSize);

        // Validate pagination parameters
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = context.Projects
            .Where(p => p.OwnerId == userId)
            .OrderByDescending(p => p.UpdatedAt);

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var projects = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProjectListItemDto(
                p.Id,
                p.Name,
                p.Description))
            .ToListAsync(cancellationToken);

        return new ProjectListResponse(
            projects,
            new PaginationMetadataDto(page, pageSize, totalItems, totalPages));
    }

    public async Task<ProjectDto> GetProjectByIdAsync(
        ProjectId projectId,
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting project {ProjectId} for user {UserId}", projectId, userId);

        var project = await context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project is null)
        {
            logger.LogWarning("Project {ProjectId} not found", projectId);
            throw new KeyNotFoundException($"Project with ID '{projectId}' was not found");
        }

        // Verify ownership
        if (project.OwnerId != userId)
        {
            logger.LogWarning("User {UserId} attempted to access project {ProjectId} owned by {OwnerId}",
                userId, projectId, project.OwnerId);
            throw new UnauthorizedAccessException("You do not have permission to access this project");
        }

        return MapToDto(project);
    }

    public async Task<ProjectDto> CreateProjectAsync(
        CreateProjectRequest request,
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating project '{ProjectName}' for user {UserId}", request.Name, userId);

        var project = new Project(
            ProjectId.New(),
            request.Name,
            request.Description,
            userId);

        context.Projects.Add(project);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Project {ProjectId} created successfully", project.Id);

        return MapToDto(project);
    }

    public async Task<ProjectDto> UpdateProjectAsync(
        ProjectId projectId,
        UpdateProjectRequest request,
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating project {ProjectId} for user {UserId}", projectId, userId);

        var project = await context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project is null)
        {
            logger.LogWarning("Project {ProjectId} not found", projectId);
            throw new KeyNotFoundException($"Project with ID '{projectId}' was not found");
        }

        // Verify ownership
        if (project.OwnerId != userId)
        {
            logger.LogWarning("User {UserId} attempted to update project {ProjectId} owned by {OwnerId}",
                userId, projectId, project.OwnerId);
            throw new UnauthorizedAccessException("You do not have permission to update this project");
        }

        // TODO: Add Update method to Project entity for proper encapsulation
        // For MVP, using EF Core's change tracking with Entry API
        context.Entry(project).Property(p => p.Name).CurrentValue = request.Name;
        context.Entry(project).Property(p => p.Description).CurrentValue = request.Description;
        context.Entry(project).Property(p => p.UpdatedAt).CurrentValue = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Project {ProjectId} updated successfully", projectId);

        return MapToDto(project);
    }

    public async Task<DeleteProjectResponse> DeleteProjectAsync(
        ProjectId projectId,
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Deleting project {ProjectId} for user {UserId}", projectId, userId);

        var project = await context.Projects
            .Include(p => p.WorkItems)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project is null)
        {
            logger.LogWarning("Project {ProjectId} not found", projectId);
            throw new KeyNotFoundException($"Project with ID '{projectId}' was not found");
        }

        // Verify ownership
        if (project.OwnerId != userId)
        {
            logger.LogWarning("User {UserId} attempted to delete project {ProjectId} owned by {OwnerId}",
                userId, projectId, project.OwnerId);
            throw new UnauthorizedAccessException("You do not have permission to delete this project");
        }

        // Count work items for response (cascade delete is handled by EF Core configuration)
        var deletedCount = project.WorkItems.Count;

        context.Projects.Remove(project);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Project {ProjectId} and {DeletedCount} work items deleted successfully",
            projectId, deletedCount);

        return new DeleteProjectResponse(
            "Project deleted successfully",
            deletedCount);
    }

    /// <summary>
    /// Maps Project entity to ProjectDto.
    /// </summary>
    private static ProjectDto MapToDto(Project project) => new(
        project.Id,
        project.Name,
        project.Description,
        project.OwnerId,
        project.CreatedAt,
        project.UpdatedAt);
}
