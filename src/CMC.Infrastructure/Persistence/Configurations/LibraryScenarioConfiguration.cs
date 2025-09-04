using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations
{
	public class LibraryScenarioConfiguration : IEntityTypeConfiguration<LibraryScenario>
	{
		public void Configure(EntityTypeBuilder<LibraryScenario> e)
		{
			e.ToTable("LibraryScenarios");
			e.HasKey(x => x.Id);

			e.Property(x => x.Name).IsRequired().HasMaxLength(200);
			e.Property(x => x.AnnualFrequency).HasPrecision(9, 6);
			e.Property(x => x.ImpactPctRevenue).HasPrecision(9, 6);
			e.Property(x => x.CreatedAt).IsRequired();
			e.Property(x => x.UpdatedAt).IsRequired();
			e.Property(x => x.IsDeleted).HasDefaultValue(false);
			e.Property(x => x.DeletedBy).HasMaxLength(320);

			e.HasIndex(x => x.Name);

			e.HasMany(x => x.TagLinks)
			 .WithOne(l => l.LibraryScenario)
			 .HasForeignKey(l => l.LibraryScenarioId)
			 .OnDelete(DeleteBehavior.Cascade);

			e.HasMany(x => x.IndustryLinks)
			 .WithOne(l => l.LibraryScenario)
			 .HasForeignKey(l => l.LibraryScenarioId)
			 .OnDelete(DeleteBehavior.Cascade);
		}
	}
}
