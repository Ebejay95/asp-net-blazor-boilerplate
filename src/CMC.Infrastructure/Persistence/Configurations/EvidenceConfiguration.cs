using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace CMC.Infrastructure.Persistence.Configurations;


public class EvidenceConfiguration : IEntityTypeConfiguration<Evidence>
{
public void Configure(EntityTypeBuilder<Evidence> e)
{
e.HasKey(x => x.Id);
e.Property(x => x.Source).IsRequired().HasMaxLength(64);
e.Property(x => x.Location).HasMaxLength(1024);
e.Property(x => x.HashSha256).HasMaxLength(128);
e.Property(x => x.Confidentiality).HasMaxLength(64);
e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
e.Property(x => x.CreatedAt).IsRequired();
e.Property(x => x.UpdatedAt).IsRequired();
e.Property(x => x.IsDeleted).HasDefaultValue(false);
e.Property(x => x.DeletedBy).HasMaxLength(320);
e.HasIndex(x => new { x.CustomerId, x.IsDeleted });
}
}
