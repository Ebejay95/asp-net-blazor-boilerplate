using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace CMC.Infrastructure.Persistence.Configurations;


public class ScenarioConfiguration : IEntityTypeConfiguration<Scenario>
{
public void Configure(EntityTypeBuilder<Scenario> e)
{
e.HasKey(x => x.Id);
e.Property(x => x.Name).IsRequired().HasMaxLength(200);
e.Property(x => x.AnnualFrequency).HasPrecision(18, 4);
e.Property(x => x.ImpactPctRevenue).HasPrecision(18, 4);
e.Property(x => x.Tags).HasMaxLength(512);


e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
e.HasOne(x => x.LibraryScenario).WithMany().HasForeignKey(x => x.LibraryScenarioId).OnDelete(DeleteBehavior.Restrict);


e.Property(x => x.CreatedAt).IsRequired();
e.Property(x => x.UpdatedAt).IsRequired();
e.Property(x => x.IsDeleted).HasDefaultValue(false);
e.Property(x => x.DeletedBy).HasMaxLength(320);


e.HasIndex(x => new { x.CustomerId, x.IsDeleted });
}
}
