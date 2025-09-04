using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CMC.Infrastructure.Persistence.Extensions;

namespace CMC.Infrastructure.Persistence.Configurations
{
	public class EvidenceConfiguration : IEntityTypeConfiguration<Evidence>
	{
		public void Configure(EntityTypeBuilder<Evidence> e)
		{
			e.ToTable("Evidence");
			e.HasKey(x => x.Id);

			// Scalar props
			e.Property(x => x.Source).IsRequired().HasMaxLength(64);
			e.Property(x => x.Location).HasMaxLength(1024);
			e.Property(x => x.HashSha256).HasMaxLength(64); // 64 hex chars
			e.Property(x => x.Confidentiality).HasMaxLength(64);

			// Zeitstempel: keine HasColumnType(...) -> provider-agnostisch (Npgsql => timestamptz)
			e.Property(x => x.CollectedAt).IsRequired();
			e.Property(x => x.ValidUntil);
			e.Property(x => x.CreatedAt).IsRequired();
			e.Property(x => x.UpdatedAt).IsRequired();

			// FK: Evidence -> Customer (kein Nav nötig)
			e.HasMany(ev => ev.Controls)
                .WithOne(c => c.Evidence)
                .HasForeignKey(c => c.EvidenceId)
                .OnDelete(DeleteBehavior.SetNull);

			// Soft delete defaults (globaler Filter kommt aus ApplySoftDeleteFilters)
			e.Property(x => x.IsDeleted).HasDefaultValue(false);
			e.Property(x => x.DeletedBy).HasMaxLength(320);

			// Concurrency: Postgres xmin
			e.UseXminAsConcurrencyToken();

			// Indizes
			e.HasIndex(x => new { x.CustomerId, x.IsDeleted });
			e.HasIndex(x => x.CollectedAt);
		}
	}
}
