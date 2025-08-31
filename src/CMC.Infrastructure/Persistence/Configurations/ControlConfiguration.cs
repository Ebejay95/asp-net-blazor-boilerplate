using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations
{
    public class ControlConfiguration : IEntityTypeConfiguration<Control>
    {
        public void Configure(EntityTypeBuilder<Control> e)
        {
            e.ToTable("Controls");
            e.HasKey(x => x.Id);

            e.HasOne(x => x.Customer)
             .WithMany(c => c.Controls)
             .HasForeignKey(x => x.CustomerId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.LibraryControl)
             .WithMany()
             .HasForeignKey(x => x.LibraryControlId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Evidence)
             .WithMany(ev => ev.Controls)
             .HasForeignKey(x => x.EvidenceId)
             .OnDelete(DeleteBehavior.SetNull);

            // ⚠️ KEIN direkter FK mehr zu Scenario; M:N via ControlScenario

            e.HasMany(x => x.ToDos)
             .WithOne()
             .HasForeignKey(t => t.ControlId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(x => x.Status).HasMaxLength(64);
            e.Property(x => x.Coverage).HasPrecision(9, 6);
            e.Property(x => x.EvidenceWeight).HasPrecision(9, 6);
            e.Property(x => x.Freshness).HasPrecision(9, 6);
            e.Property(x => x.CostTotalEur).HasPrecision(18, 2);
            e.Property(x => x.DeltaEalEur).HasPrecision(18, 2);
            e.Property(x => x.Score).HasPrecision(9, 4);
            e.Property(x => x.CreatedAt).IsRequired();
            e.Property(x => x.UpdatedAt).IsRequired();
            e.Property(x => x.DeletedBy).HasMaxLength(320);
            e.Property(x => x.IsDeleted).HasDefaultValue(false);

            e.HasIndex(x => new { x.CustomerId, x.IsDeleted });
            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.LibraryControlId);

        }
    }
}
