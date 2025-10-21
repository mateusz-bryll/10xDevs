using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Modules.Users;
using TaskFlow.Modules.WorkItems.Domain.Entities;
using TaskFlow.Modules.WorkItems.Domain.ValueObjects;

namespace TaskFlow.Modules.WorkItems.Infrastructure.Persistence.Configurations;

public sealed class WorkItemConfiguration : IEntityTypeConfiguration<WorkItem>
{
    public void Configure(EntityTypeBuilder<WorkItem> b)
    {
        b.ToTable("work_items");

        // Primary Key
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasConversion(new WorkItemId.EfCoreValueConverter())
            .ValueGeneratedNever();

        // Foreign Keys
        b.Property(x => x.ProjectId)
            .IsRequired()
            .HasConversion(new ProjectId.EfCoreValueConverter());

        b.Property(x => x.ParentId)
            .HasConversion(new WorkItemId.EfCoreValueConverter());

        // Enums (stored as strings)
        b.Property(x => x.WorkItemType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        b.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // Properties
        b.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        b.Property(x => x.Description)
            .HasMaxLength(5000);

        b.Property(x => x.AssignedUserId)
            .HasConversion(new UserId.EfCoreValueConverter())
            .HasMaxLength(256);

        b.Property(x => x.CreatedAt)
            .IsRequired();

        b.Property(x => x.UpdatedAt)
            .IsRequired();

        // Self-referencing relationship
        b.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Concurrency
        b.Property<uint>("xmin")
            .IsRowVersion()
            .HasColumnType("xid");
    }
}
