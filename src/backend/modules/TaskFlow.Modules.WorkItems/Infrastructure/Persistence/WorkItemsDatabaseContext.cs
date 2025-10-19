using Microsoft.EntityFrameworkCore;
using TaskFlow.Modules.WorkItems.Domain.Entities;

namespace TaskFlow.Modules.WorkItems.Infrastructure.Persistence;

public sealed class WorkItemsDatabaseContext : DbContext
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();

    public WorkItemsDatabaseContext(DbContextOptions<WorkItemsDatabaseContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all IEntityTypeConfiguration<T> from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkItemsDatabaseContext).Assembly);
    }
}
