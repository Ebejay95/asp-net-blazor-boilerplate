using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations
{
	public class LibraryControlConfiguration : IEntityTypeConfiguration<LibraryControl>
	{
		public void Configure(EntityTypeBuilder<LibraryControl> e)
		{
			e.ToTable("LibraryControls");
			e.HasKey(x => x.Id);

			// Properties
			e.Property(x => x.Name).IsRequired().HasMaxLength(200);
			e.Property(x => x.CapexEur).HasPrecision(18, 2);
			e.Property(x => x.OpexYearEur).HasPrecision(18, 2);
			e.Property(x => x.InternalDays).IsRequired();
			e.Property(x => x.ExternalDays).IsRequired();
			e.Property(x => x.TotalDays).IsRequired();
			e.Property(x => x.CreatedAt).IsRequired();
			e.Property(x => x.UpdatedAt).IsRequired();
			e.Property(x => x.IsDeleted).HasDefaultValue(false);
			e.Property(x => x.DeletedBy).HasMaxLength(320);

			// Soft Delete Filter - WICHTIG: Gelöschte Entitäten ausblenden
			e.HasQueryFilter(x => !x.IsDeleted);

			// Indizes - Name + IsDeleted für bessere Performance bei Filterung
			e.HasIndex(x => new { x.Name, x.IsDeleted });
			e.HasIndex(x => x.IsDeleted); // Separater Index für reine Soft-Delete-Abfragen

			// n:m LibraryControl <-> Tag via LibraryControlTag
			e.HasMany(x => x.TagLinks)
			 .WithOne(l => l.LibraryControl)
			 .HasForeignKey(l => l.LibraryControlId)
			 .OnDelete(DeleteBehavior.Cascade);

			// n:m LibraryControl <-> Industry via LibraryControlIndustry
			e.HasMany(x => x.IndustryLinks)
			 .WithOne(l => l.LibraryControl)
			 .HasForeignKey(l => l.LibraryControlId)
			 .OnDelete(DeleteBehavior.Cascade);

			// n:m LibraryControl <-> LibraryScenario via LibraryControlScenario (mit Effekten)
			e.HasMany(x => x.ScenarioLinks)
			 .WithOne(l => l.LibraryControl)
			 .HasForeignKey(l => l.LibraryControlId)
			 .OnDelete(DeleteBehavior.Cascade);

			// n:m LibraryControl <-> Framework via FrameworkLibraryControl
			e.HasMany(x => x.FrameworkLinks)
			 .WithOne(l => l.LibraryControl)
			 .HasForeignKey(l => l.LibraryControlId)
			 .OnDelete(DeleteBehavior.Cascade);
		}
	}
}
