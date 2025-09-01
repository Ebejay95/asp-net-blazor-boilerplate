using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations
{
	public class FrameworkIndustryConfiguration : IEntityTypeConfiguration<FrameworkIndustry>
	{
		public void Configure(EntityTypeBuilder<FrameworkIndustry> e)
		{
			e.ToTable("FrameworkIndustries");
			e.HasKey(x => new { x.FrameworkId, x.IndustryId });

			e.HasOne(x => x.Framework)
				.WithMany(f => f.IndustryLinks)
				.HasForeignKey(x => x.FrameworkId)
				.OnDelete(DeleteBehavior.Cascade);

			e.HasOne(x => x.Industry)
				.WithMany(i => i.FrameworkIndustries)
				.HasForeignKey(x => x.IndustryId)
				.OnDelete(DeleteBehavior.Cascade);

			e.HasIndex(x => x.IndustryId);
		}
	}
}
