using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations
{
	public class ScenarioTagConfiguration : IEntityTypeConfiguration<ScenarioTag>
	{
		public void Configure(EntityTypeBuilder<ScenarioTag> e)
		{
			e.ToTable("ScenarioTags");
			e.HasKey(x => new { x.ScenarioId, x.TagId });

			e.HasOne(x => x.Scenario)
				.WithMany(s => s.TagLinks)
				.HasForeignKey(x => x.ScenarioId)
				.OnDelete(DeleteBehavior.Cascade);

			e.HasOne(x => x.Tag)
				.WithMany()
				.HasForeignKey(x => x.TagId)
				.OnDelete(DeleteBehavior.Cascade);

			e.HasIndex(x => x.TagId);
		}
	}
}
