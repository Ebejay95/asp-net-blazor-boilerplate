using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace CMC.Infrastructure.Persistence.Configurations;


public class LibraryScenarioConfiguration : IEntityTypeConfiguration<LibraryScenario>
{
public void Configure(EntityTypeBuilder<LibraryScenario> entity)
{
entity.HasKey(e => e.Id);
entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
entity.Property(e => e.AnnualFrequency).HasPrecision(18, 4);
entity.Property(e => e.ImpactPctRevenue).HasPrecision(18, 4);
entity.Property(e => e.Tags).HasMaxLength(512);
entity.Property(e => e.CreatedAt).IsRequired();
entity.Property(e => e.UpdatedAt).IsRequired();


entity.Property(e => e.IsDeleted).HasDefaultValue(false);
entity.Property(e => e.DeletedBy).HasMaxLength(320);
}
}
