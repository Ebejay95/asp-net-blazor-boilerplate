using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations;

public class ToDoConfiguration : IEntityTypeConfiguration<ToDo>
{
    public void Configure(EntityTypeBuilder<ToDo> e)
    {
        e.HasKey(x => x.Id);
        e.Property(x => x.ControlId).IsRequired();
        e.Property(x => x.DependsOnTaskId);
        e.Property(x => x.Name).IsRequired().HasMaxLength(200);
        e.Property(x => x.Status).HasMaxLength(64);
        e.Property(x => x.Assignee).HasMaxLength(320);
        e.Property(x => x.CreatedAt).IsRequired();
        e.Property(x => x.UpdatedAt).IsRequired();
        e.Property(x => x.IsDeleted).HasDefaultValue(false);
        e.Property(x => x.DeletedBy).HasMaxLength(320);
    }
}
