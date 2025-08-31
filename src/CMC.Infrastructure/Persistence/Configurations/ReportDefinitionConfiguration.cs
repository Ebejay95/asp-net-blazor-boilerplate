using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations
{
	public class ReportDefinitionConfiguration : IEntityTypeConfiguration<ReportDefinition>
	{
		public void Configure(EntityTypeBuilder<ReportDefinition> e)
		{
			e.ToTable("ReportDefinitions");
			e.HasKey(x => x.Id);

			// FK
			e.HasOne<Customer>()
				.WithMany()
				.HasForeignKey(x => x.CustomerId)
				.OnDelete(DeleteBehavior.Restrict);

			e.Property(x => x.Name).IsRequired().HasMaxLength(200);
			e.Property(x => x.Kind).IsRequired().HasMaxLength(64);
			e.Property(x => x.WindowDays).IsRequired();
			// Provider-neutral lassen (kein HasColumnType), außer du willst bewusst jsonb/text erzwingen
			e.Property(x => x.Sections);

			e.Property(x => x.CreatedAt).IsRequired();
			e.Property(x => x.UpdatedAt).IsRequired();
			e.Property(x => x.IsDeleted).HasDefaultValue(false);
			e.Property(x => x.DeletedBy).HasMaxLength(320);

			// Concurrency
			e.UseXminAsConcurrencyToken();

			// pro Kunde eindeutiger Name
			e.HasIndex(x => new { x.CustomerId, x.Name }).IsUnique();

			// Optional: erlaubte Kinds durchsetzen
			// e.HasCheckConstraint("CK_ReportDefinitions_Kind", "LOWER(\"Kind\") IN ('daily','weekly','monthly','window')");
		}
	}
}
