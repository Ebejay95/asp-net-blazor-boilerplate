using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations.Joins
{
    public sealed class ProvisionedScenarioMapConfiguration : IEntityTypeConfiguration<ProvisionedScenarioMap>
    {
        public void Configure(EntityTypeBuilder<ProvisionedScenarioMap> e)
        {
            e.ToTable("ProvisionedScenarioMaps");
            e.HasKey(x => new { x.CustomerId, x.LibraryScenarioId });

            e.Property(x => x.CreatedAt).IsRequired();

            // FIXED: Customer löschen -> Maps mitlöschen
            e.HasOne<Customer>()
             .WithMany()
             .HasForeignKey(x => x.CustomerId)
             .OnDelete(DeleteBehavior.Cascade);

            // Library bleibt Restrict - soll nicht aus Versehen gelöscht werden
            e.HasOne<LibraryScenario>()
             .WithMany()
             .HasForeignKey(x => x.LibraryScenarioId)
             .OnDelete(DeleteBehavior.Restrict);

            // FIXED: Scenario löschen -> Maps mitlöschen (war vorher Restrict)
            e.HasOne<Scenario>()
             .WithMany()
             .HasForeignKey(x => x.ScenarioId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.ScenarioId).IsUnique();
            e.HasIndex(x => x.LibraryScenarioId);
        }
    }
}
