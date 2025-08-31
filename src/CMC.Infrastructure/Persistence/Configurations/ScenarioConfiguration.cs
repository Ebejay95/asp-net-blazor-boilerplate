using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations
{
	public class ScenarioConfiguration : IEntityTypeConfiguration<Scenario>
	{
		public void Configure(EntityTypeBuilder<Scenario> e)
		{
			e.ToTable("Scenarios");
			e.HasKey(x => x.Id);

			e.Property(x => x.Name).IsRequired().HasMaxLength(200);
			e.Property(x => x.AnnualFrequency).HasPrecision(9, 6);
			e.Property(x => x.ImpactPctRevenue).HasPrecision(9, 6);
			e.Property(x => x.CreatedAt).IsRequired();
			e.Property(x => x.UpdatedAt).IsRequired();
			e.Property(x => x.IsDeleted).HasDefaultValue(false);
			e.Property(x => x.DeletedBy).HasMaxLength(320);

			e.HasOne(x => x.Customer)
			 .WithMany()
			 .HasForeignKey(x => x.CustomerId)
			 .OnDelete(DeleteBehavior.Restrict);

			e.HasOne(x => x.LibraryScenario)
			 .WithMany()
			 .HasForeignKey(x => x.LibraryScenarioId)
			 .OnDelete(DeleteBehavior.Restrict);

			e.HasMany(x => x.TagLinks)
			 .WithOne(l => l.Scenario)
			 .HasForeignKey(l => l.ScenarioId)
			 .OnDelete(DeleteBehavior.Cascade);

			e.HasIndex(x => new { x.CustomerId, x.IsDeleted });

            e.HasIndex(x => new { x.CustomerId, x.LibraryScenarioId })
             .IsUnique()
             .HasDatabaseName("UX_Scenarios_Cust_LibScenario")
             .HasFilter("\"IsDeleted\" = false");
		}
	}
}
