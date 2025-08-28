using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations;

public class LibraryFrameworkConfiguration : IEntityTypeConfiguration<LibraryFramework>
{
    public void Configure(EntityTypeBuilder<LibraryFramework> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(e => e.Version)
            .IsRequired()
            .HasMaxLength(64);

        entity.Property(e => e.Industry)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(e => e.CreatedAt).IsRequired();
        entity.Property(e => e.UpdatedAt).IsRequired();

        // schneller Lookup
        entity.HasIndex(e => new { e.Name, e.Version }).IsUnique(false);
    }
}
