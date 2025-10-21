using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Modules.WorkItems.Application.Services;
using TaskFlow.Modules.WorkItems.Infrastructure.Persistence;

namespace TaskFlow.Modules.WorkItems;

/// <summary>
/// Extension methods for registering WorkItems module services in dependency injection.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registers all WorkItems module services, validators, and infrastructure in the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddWorkItemsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register application services
        services.AddScoped<IProjectsService, ProjectsService>();
        services.AddScoped<IWorkItemsService, WorkItemsService>();

        // Register FluentValidation validators
        // Automatically discovers and registers all validators in the assembly
        services.AddValidatorsFromAssemblyContaining<WorkItemsDatabaseContext>();

        return services;
    }
}
