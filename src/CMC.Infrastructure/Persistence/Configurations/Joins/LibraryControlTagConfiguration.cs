using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations
{
	public class LibraryControlTagConfiguration : IEntityTypeConfiguration<LibraryControlTag>
	{
		public void Configure(EntityTypeBuilder<LibraryControlTag> e)
		{
			e.ToTable("LibraryControlTags");
			e.HasKey(x => new { x.LibraryControlId, x.TagId });

			e.HasOne(x => x.LibraryControl)
				.WithMany(lc => lc.TagLinks)
				.HasForeignKey(x => x.LibraryControlId)
				.OnDelete(DeleteBehavior.Cascade);

			e.HasOne(x => x.Tag)
				.WithMany()
				.HasForeignKey(x => x.TagId)
				.OnDelete(DeleteBehavior.Cascade);

			e.HasIndex(x => x.TagId);
		}
	}
}
