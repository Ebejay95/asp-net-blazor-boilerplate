using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations
{
	public class LibraryControlFrameworkConfiguration : IEntityTypeConfiguration<LibraryControlFramework>
	{
		public void Configure(EntityTypeBuilder<LibraryControlFramework> e)
		{
			e.ToTable("LibraryControlFrameworks");
			e.HasKey(x => new { x.LibraryControlId, x.FrameworkId });

			e.HasOne(x => x.LibraryControl)
				.WithMany(lc => lc.FrameworkLinks)
				.HasForeignKey(x => x.LibraryControlId)
				.OnDelete(DeleteBehavior.Cascade);

			e.HasOne(x => x.Framework)
				.WithMany(f => f.ControlLinks)
				.HasForeignKey(x => x.FrameworkId)
				.OnDelete(DeleteBehavior.Cascade);

			e.HasIndex(x => x.FrameworkId);
		}
	}
}
