using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace CMC.Infrastructure.Persistence.Configurations;


public class ControlConfiguration : IEntityTypeConfiguration<Control>
{
public void Configure(EntityTypeBuilder<Control> e)
{
e.HasKey(x => x.Id);


e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
e.HasOne(x => x.LibraryControl).WithMany().HasForeignKey(x => x.LibraryControlId).OnDelete(DeleteBehavior.Restrict);
e.HasOne(x => x.Evidence).WithMany().HasForeignKey(x => x.EvidenceId).OnDelete(DeleteBehavior.SetNull);


e.Property(x => x.Coverage).HasPrecision(18, 4);
e.Property(x => x.EvidenceWeight).HasPrecision(18, 4);
e.Property(x => x.Freshness).HasPrecision(18, 4);
e.Property(x => x.CostTotalEur).HasPrecision(18, 2);
e.Property(x => x.DeltaEalEur).HasPrecision(18, 2);
e.Property(x => x.Score).HasPrecision(18, 4);
e.Property(x => x.Status).HasMaxLength(64);


e.Property(x => x.CreatedAt).IsRequired();
e.Property(x => x.UpdatedAt).IsRequired();


e.Property(x => x.DeletedBy).HasMaxLength(320);
e.Property(x => x.IsDeleted).HasDefaultValue(false);


e.HasIndex(x => new { x.CustomerId, x.IsDeleted });
}
}
