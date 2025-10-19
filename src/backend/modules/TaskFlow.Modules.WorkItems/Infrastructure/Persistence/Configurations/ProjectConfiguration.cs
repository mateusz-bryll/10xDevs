using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Modules.WorkItems.Domain.Entities;
using TaskFlow.Modules.WorkItems.Domain.ValueObjects;

namespace TaskFlow.Modules.WorkItems.Infrastructure.Persistence.Configurations;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> b)
    {
        b.ToTable("projects");

        // Primary Key
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasConversion(new ProjectId.EfCoreValueConverter())
            .ValueGeneratedNever();

        // Properties
        b.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        b.Property(x => x.Description)
            .HasMaxLength(2000);

        b.Property(x => x.OwnerId)
            .IsRequired()
            .HasMaxLength(256);

        b.Property(x => x.CreatedAt)
            .IsRequired();

        b.Property(x => x.UpdatedAt)
            .IsRequired();

        // Relationships
        b.HasMany(x => x.WorkItems)
            .WithOne(x => x.Project)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Concurrency
        b.Property<uint>("xmin")
            .IsRowVersion()
            .HasColumnType("xid");
    }
}
