using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TaskFlow.Modules.Users;
using TaskFlow.Modules.WorkItems.Application.Requests;
using TaskFlow.Modules.WorkItems.Application.Responses;
using TaskFlow.Modules.WorkItems.Application.Services;
using TaskFlow.Modules.WorkItems.Domain.ValueObjects;

namespace TaskFlow.Modules.WorkItems;

public static class Endpoints
{
    public static IEndpointRouteBuilder UseWorkItemsModule(this IEndpointRouteBuilder builder)
    {
        var projectsEndpoints = builder.MapGroup("api/projects");

        // 2.2.1 List User Projects
        projectsEndpoints.MapGet("", ListUserProjects)
            .WithName("ListUserProjects")
            .Produces<ProjectListResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        // 2.2.2 Create Project
        projectsEndpoints.MapPost("", CreateProject)
            .WithName("CreateProject")
            .Produces<ProjectDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        // 2.2.3 Get Project by ID
        projectsEndpoints.MapGet("{projectId}", GetProjectById)
            .WithName("GetProjectById")
            .Produces<ProjectDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // 2.2.4 Update Project
        projectsEndpoints.MapPut("{projectId}", UpdateProject)
            .WithName("UpdateProject")
            .Produces<ProjectDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // 2.2.5 Delete Project
        projectsEndpoints.MapDelete("{projectId}", DeleteProject)
            .WithName("DeleteProject")
            .Produces<DeleteProjectResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return builder;
    }

    /// <summary>
    /// 2.2.1 List User Projects
    /// GET /api/projects
    /// Retrieves paginated list of projects owned by the authenticated user.
    /// </summary>
    private static async Task<IResult> ListUserProjects(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromServices] IProjectsService projectsService,
        [FromServices] ICurrentUserAccessor currentUserAccessor,
        CancellationToken cancellationToken)
    {
        // Get current authenticated user
        var currentUser = currentUserAccessor.GetCurrentUser();

        // List projects (service handles pagination defaults and validation)
        var response = await projectsService.ListProjectsAsync(
            currentUser.Id,
            page > 0 ? page : 1,
            pageSize > 0 ? pageSize : 20,
            cancellationToken);

        return Results.Ok(response);
    }

    /// <summary>
    /// 2.2.2 Create Project
    /// POST /api/projects
    /// Creates a new project owned by the authenticated user.
    /// </summary>
    private static async Task<IResult> CreateProject(
        [FromBody] CreateProjectRequest request,
        [FromServices] IProjectsService projectsService,
        [FromServices] IValidator<CreateProjectRequest> validator,
        [FromServices] ICurrentUserAccessor currentUserAccessor,
        CancellationToken cancellationToken)
    {
        // Validate request
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(
                validationResult.ToDictionary(),
                title: "One or more validation errors occurred",
                statusCode: StatusCodes.Status400BadRequest);
        }

        // Get current authenticated user
        var currentUser = currentUserAccessor.GetCurrentUser();

        // Create project
        var projectDto = await projectsService.CreateProjectAsync(
            request,
            currentUser.Id,
            cancellationToken);

        // Return 201 Created with Location header
        return Results.Created(
            $"/api/projects/{projectDto.Id}",
            projectDto);
    }

    /// <summary>
    /// 2.2.3 Get Project by ID
    /// GET /api/projects/{projectId}
    /// Retrieves a specific project by ID (user must be owner).
    /// </summary>
    private static async Task<IResult> GetProjectById(
        ProjectId projectId,
        [FromServices] IProjectsService projectsService,
        [FromServices] ICurrentUserAccessor currentUserAccessor,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get current authenticated user
            var currentUser = currentUserAccessor.GetCurrentUser();

            // Get project (service handles ownership verification)
            var projectDto = await projectsService.GetProjectByIdAsync(
                projectId,
                currentUser.Id,
                cancellationToken);

            return Results.Ok(projectDto);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Problem(
                title: "Forbidden",
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden);
        }
    }

    /// <summary>
    /// 2.2.4 Update Project
    /// PUT /api/projects/{projectId}
    /// Updates an existing project (user must be owner).
    /// </summary>
    private static async Task<IResult> UpdateProject(
        ProjectId projectId,
        [FromBody] UpdateProjectRequest request,
        [FromServices] IProjectsService projectsService,
        [FromServices] IValidator<UpdateProjectRequest> validator,
        [FromServices] ICurrentUserAccessor currentUserAccessor,
        CancellationToken cancellationToken)
    {
        // Validate request
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(
                validationResult.ToDictionary(),
                title: "One or more validation errors occurred",
                statusCode: StatusCodes.Status400BadRequest);
        }

        try
        {
            // Get current authenticated user
            var currentUser = currentUserAccessor.GetCurrentUser();

            // Update project (service handles ownership verification)
            var projectDto = await projectsService.UpdateProjectAsync(
                projectId,
                request,
                currentUser.Id,
                cancellationToken);

            return Results.Ok(projectDto);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Problem(
                title: "Forbidden",
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden);
        }
    }

    /// <summary>
    /// 2.2.5 Delete Project
    /// DELETE /api/projects/{projectId}
    /// Deletes a project and all its work items (cascade delete, user must be owner).
    /// </summary>
    private static async Task<IResult> DeleteProject(
        ProjectId projectId,
        [FromServices] IProjectsService projectsService,
        [FromServices] ICurrentUserAccessor currentUserAccessor,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get current authenticated user
            var currentUser = currentUserAccessor.GetCurrentUser();

            // Delete project (service handles ownership verification and cascade)
            var response = await projectsService.DeleteProjectAsync(
                projectId,
                currentUser.Id,
                cancellationToken);

            return Results.Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Problem(
                title: "Forbidden",
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden);
        }
    }
}