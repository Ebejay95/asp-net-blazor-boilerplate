using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations;

public class ToDoConfiguration : IEntityTypeConfiguration<ToDo>
{
    public void Configure(EntityTypeBuilder<ToDo> e)
    {
        e.ToTable("ToDos");
        e.HasKey(x => x.Id);

        e.Property(x => x.ControlId).IsRequired();
        e.Property(x => x.DependsOnTaskId);

        e.Property(x => x.Name).IsRequired().HasMaxLength(200);
        e.Property(x => x.Status)
         .HasConversion<string>()
         .HasMaxLength(32);

        e.Property(x => x.Assignee).HasMaxLength(320);
        e.Property(x => x.CreatedAt).IsRequired();
        e.Property(x => x.UpdatedAt).IsRequired();
        e.Property(x => x.IsDeleted).HasDefaultValue(false);
        e.Property(x => x.DeletedBy).HasMaxLength(320);

        e.HasOne<Control>()
         .WithMany(c => c.ToDos)
         .HasForeignKey(x => x.ControlId)
         .OnDelete(DeleteBehavior.Restrict);

        e.HasOne<ToDo>()
         .WithMany()
         .HasForeignKey(x => x.DependsOnTaskId)
         .OnDelete(DeleteBehavior.SetNull);

        e.HasIndex(x => x.ControlId);
        e.HasIndex(x => new { x.ControlId, x.DependsOnTaskId });
        e.HasIndex(x => new { x.Status, x.IsDeleted });
    }
}
