using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations
{
	public class LibraryScenarioIndustryConfiguration : IEntityTypeConfiguration<LibraryScenarioIndustry>
	{
		public void Configure(EntityTypeBuilder<LibraryScenarioIndustry> e)
		{
			e.ToTable("LibraryScenarioIndustries");
			e.HasKey(x => new { x.LibraryScenarioId, x.IndustryId });

			e.HasOne(x => x.LibraryScenario)
				.WithMany(ls => ls.IndustryLinks)
				.HasForeignKey(x => x.LibraryScenarioId)
				.OnDelete(DeleteBehavior.Cascade);

			e.HasOne(x => x.Industry)
				.WithMany()
				.HasForeignKey(x => x.IndustryId)
				.OnDelete(DeleteBehavior.Cascade);

			e.HasIndex(x => x.IndustryId);
		}
	}
}
