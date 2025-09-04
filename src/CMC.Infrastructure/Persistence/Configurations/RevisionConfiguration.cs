using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CMC.Domain.Entities;

namespace CMC.Infrastructure.Persistence.Configurations;

public sealed class RevisionConfiguration : IEntityTypeConfiguration<Revision>
{
    public void Configure(EntityTypeBuilder<Revision> b)
    {
        b.ToTable("Revisions");
        b.HasKey(x => x.Id);

        // Auto-increment (Npgsql Identity by default)
        b.Property(x => x.Id).ValueGeneratedOnAdd();

        b.Property(x => x.Table)
            .IsRequired();

        b.Property(x => x.AssetId)
            .IsRequired();

        b.Property(x => x.Action)
            .HasMaxLength(32)
            .IsRequired();

        b.Property(x => x.UserEmail)
            .HasMaxLength(320);

        b.Property(x => x.Data)
            .HasColumnType("jsonb")
            .IsRequired();

        b.Property(x => x.CreatedAt)
            .IsRequired();

        b.HasIndex(x => new { x.Table, x.AssetId, x.CreatedAt });
    }
}
