using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace CMC.Infrastructure.Persistence.Configurations;


public class FrameworkConfiguration : IEntityTypeConfiguration<Framework>
{
public void Configure(EntityTypeBuilder<Framework> entity)
{
entity.HasKey(e => e.Id);


entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
entity.Property(e => e.Version).IsRequired().HasMaxLength(64);
entity.Property(e => e.CreatedAt).IsRequired();
entity.Property(e => e.UpdatedAt).IsRequired();


// Soft delete fields
entity.Property(e => e.IsDeleted).HasDefaultValue(false);
entity.Property(e => e.DeletedBy).HasMaxLength(320);


entity.HasIndex(e => new { e.Name, e.Version }).IsUnique(false);
entity.HasIndex(e => e.IsDeleted);
}
}
