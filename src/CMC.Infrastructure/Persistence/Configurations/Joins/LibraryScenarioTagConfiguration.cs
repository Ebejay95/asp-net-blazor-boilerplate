using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations
{
	public class LibraryScenarioTagConfiguration : IEntityTypeConfiguration<LibraryScenarioTag>
	{
		public void Configure(EntityTypeBuilder<LibraryScenarioTag> e)
		{
			e.ToTable("LibraryScenarioTags");
			e.HasKey(x => new { x.LibraryScenarioId, x.TagId });

			e.HasOne(x => x.LibraryScenario)
				.WithMany(ls => ls.TagLinks)
				.HasForeignKey(x => x.LibraryScenarioId)
				.OnDelete(DeleteBehavior.Cascade);

			e.HasOne(x => x.Tag)
				.WithMany()
				.HasForeignKey(x => x.TagId)
				.OnDelete(DeleteBehavior.Cascade);

			e.HasIndex(x => x.TagId);
		}
	}
}
