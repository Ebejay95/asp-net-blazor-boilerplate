using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations
{
	public class FrameworkConfiguration : IEntityTypeConfiguration<Framework>
	{
		public void Configure(EntityTypeBuilder<Framework> e)
		{
			e.ToTable("Frameworks");
			e.HasKey(x => x.Id);

			e.Property(x => x.Name).IsRequired().HasMaxLength(200);
			e.Property(x => x.Version).IsRequired().HasMaxLength(64);
			e.Property(x => x.CreatedAt).IsRequired();
			e.Property(x => x.UpdatedAt).IsRequired();
			e.Property(x => x.IsDeleted).HasDefaultValue(false);
			e.Property(x => x.DeletedBy).HasMaxLength(320);

			// Globaler QueryFilter: blende Soft-Deleted aus
			e.HasQueryFilter(x => !x.IsDeleted);

			// Eindeutiger Index (optional unique) + Filter auf nicht gelöschte
			e.HasIndex(x => new { x.Name, x.Version })
			 .IsUnique()
			 .HasDatabaseName("UX_Frameworks_Name_Version_Active")
			 .HasFilter("\"IsDeleted\" = false");

			// n:m Framework <-> Industry
			e.HasMany(x => x.IndustryLinks)
			 .WithOne(l => l.Framework)
			 .HasForeignKey(l => l.FrameworkId)
			 .OnDelete(DeleteBehavior.Cascade);

			// n:m Framework <-> LibraryControl
			e.HasMany(x => x.ControlLinks)
			 .WithOne(l => l.Framework)
			 .HasForeignKey(l => l.FrameworkId)
			 .OnDelete(DeleteBehavior.Cascade);
		}
	}
}
