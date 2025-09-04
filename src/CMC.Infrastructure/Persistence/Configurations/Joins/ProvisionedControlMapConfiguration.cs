using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations.Joins
{
    public sealed class ProvisionedControlMapConfiguration : IEntityTypeConfiguration<ProvisionedControlMap>
    {
        public void Configure(EntityTypeBuilder<ProvisionedControlMap> e)
        {
            e.ToTable("ProvisionedControlMaps");
            e.HasKey(x => new { x.CustomerId, x.LibraryControlId, x.ScenarioId });

            e.Property(x => x.CreatedAt).IsRequired();

            // FIXED: Customer löschen -> Maps mitlöschen
            e.HasOne<Customer>()
             .WithMany()
             .HasForeignKey(x => x.CustomerId)
             .OnDelete(DeleteBehavior.Cascade);

            // Library bleibt Restrict - soll nicht aus Versehen gelöscht werden
            e.HasOne<LibraryControl>()
             .WithMany()
             .HasForeignKey(x => x.LibraryControlId)
             .OnDelete(DeleteBehavior.Restrict);

            // FIXED: Scenario löschen -> Maps mitlöschen (war vorher Restrict)
            e.HasOne<Scenario>()
             .WithMany()
             .HasForeignKey(x => x.ScenarioId)
             .OnDelete(DeleteBehavior.Cascade);

            // FIXED: Control löschen -> Maps mitlöschen (war vorher Restrict)
            e.HasOne<Control>()
             .WithMany()
             .HasForeignKey(x => x.ControlId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.ControlId).IsUnique();
            e.HasIndex(x => x.LibraryControlId);
            e.HasIndex(x => x.ScenarioId);
        }
    }
}
