using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace CMC.Infrastructure.Persistence.Configurations;


public class LibraryControlConfiguration : IEntityTypeConfiguration<LibraryControl>
{
public void Configure(EntityTypeBuilder<LibraryControl> entity)
{
entity.HasKey(e => e.Id);


entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
entity.Property(e => e.Tag).IsRequired().HasMaxLength(100);
entity.Property(e => e.CapexEur).HasPrecision(18, 2);
entity.Property(e => e.OpexYearEur).HasPrecision(18, 2);
entity.Property(e => e.InternalDays).IsRequired();
entity.Property(e => e.ExternalDays).IsRequired();
entity.Property(e => e.TotalDays).IsRequired();
entity.Property(e => e.CreatedAt).IsRequired();
entity.Property(e => e.UpdatedAt).IsRequired();


// Soft delete
entity.Property(e => e.IsDeleted).HasDefaultValue(false);
entity.Property(e => e.DeletedBy).HasMaxLength(320);


entity.HasIndex(e => e.Tag);
}
}
