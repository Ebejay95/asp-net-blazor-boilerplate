using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations
{
	public class ReportConfiguration : IEntityTypeConfiguration<Report>
	{
		public void Configure(EntityTypeBuilder<Report> e)
		{
			e.ToTable("Reports");
			e.HasKey(x => x.Id);

			// FKs (Navigations brauchst du nicht zwingend in der Domain)
			e.HasOne<Customer>()
				.WithMany()
				.HasForeignKey(x => x.CustomerId)
				.OnDelete(DeleteBehavior.SetNull);

			e.HasOne<ReportDefinition>()
				.WithMany()
				.HasForeignKey(x => x.DefinitionId)
				.OnDelete(DeleteBehavior.Restrict);

			// Scalar
			e.Property(x => x.Frozen).IsRequired();
			e.Property(x => x.CreatedAt).IsRequired();
			e.Property(x => x.UpdatedAt).IsRequired();
			e.Property(x => x.IsDeleted).HasDefaultValue(false);
			e.Property(x => x.DeletedBy).HasMaxLength(320);

			// Concurrency (Postgres: xmin)
			e.UseXminAsConcurrencyToken();

			// Check: PeriodEnd >= PeriodStart
			e.HasCheckConstraint("CK_Reports_Period", "\"PeriodEnd\" >= \"PeriodStart\"");

			// Indizes
			e.HasIndex(x => x.DefinitionId);
			e.HasIndex(x => new { x.CustomerId, x.IsDeleted });
			e.HasIndex(x => x.GeneratedAt);

			// Eindeutigkeit (optional, empfehlenswert): gleiche Definition & Zeitraum nur 1× (unter Nicht-gelöschten)
			e.HasIndex(x => new { x.DefinitionId, x.PeriodStart, x.PeriodEnd, x.CustomerId })
				.IsUnique()
				.HasDatabaseName("UX_Reports_Def_Period_Cust")
				.HasFilter("\"IsDeleted\" = false");
		}
	}
}
