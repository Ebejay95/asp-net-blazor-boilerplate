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

            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            // FIXED: Changed from Restrict to Cascade - Controls are owned by Customer
            e.HasOne(x => x.Customer)
             .WithMany(c => c.Controls)
             .HasForeignKey(x => x.CustomerId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.LibraryControl)
             .WithMany()
             .HasForeignKey(x => x.LibraryControlId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Evidence)
             .WithMany(ev => ev.Controls)
             .HasForeignKey(x => x.EvidenceId)
             .OnDelete(DeleteBehavior.SetNull);

            // M:N
            e.HasMany(x => x.ScenarioLinks)
             .WithOne(l => l.Control)
             .HasForeignKey(l => l.ControlId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.TagLinks)
             .WithOne(l => l.Control)
             .HasForeignKey(l => l.ControlId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.IndustryLinks)
             .WithOne(l => l.Control)
             .HasForeignKey(l => l.ControlId)
             .OnDelete(DeleteBehavior.Cascade);

            // ToDos beim Löschen des Controls mitlöschen
            e.HasMany(x => x.ToDos)
             .WithOne()
             .HasForeignKey(t => t.ControlId)
             .OnDelete(DeleteBehavior.Cascade);

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
