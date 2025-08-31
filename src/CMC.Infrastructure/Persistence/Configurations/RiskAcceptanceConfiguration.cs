using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations
{
	public class RiskAcceptanceConfiguration : IEntityTypeConfiguration<RiskAcceptance>
	{
		public void Configure(EntityTypeBuilder<RiskAcceptance> e)
		{
			e.ToTable("RiskAcceptances");
			e.HasKey(x => x.Id);

			// FKs (ohne Navs in Domain)
			e.HasOne<Customer>()
				.WithMany()
				.HasForeignKey(x => x.CustomerId)
				.OnDelete(DeleteBehavior.Restrict);

			e.HasOne<Control>()
				.WithMany()
				.HasForeignKey(x => x.ControlId)
				.OnDelete(DeleteBehavior.Restrict);

			e.Property(x => x.Reason).IsRequired().HasMaxLength(1024);
			e.Property(x => x.RiskAcceptedBy).IsRequired().HasMaxLength(128);
			e.Property(x => x.CreatedAt).IsRequired();
			e.Property(x => x.UpdatedAt).IsRequired();
			e.Property(x => x.IsDeleted).HasDefaultValue(false);
			e.Property(x => x.DeletedBy).HasMaxLength(320);

			// Concurrency
			e.UseXminAsConcurrencyToken();

			// Indizes
			e.HasIndex(x => new { x.CustomerId, x.ControlId });
			e.HasIndex(x => x.ExpiresAt);
		}
	}
}
