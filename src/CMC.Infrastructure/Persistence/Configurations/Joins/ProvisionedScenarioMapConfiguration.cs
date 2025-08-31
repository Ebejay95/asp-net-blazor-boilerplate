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

            e.HasOne<Customer>()
             .WithMany()
             .HasForeignKey(x => x.CustomerId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<LibraryScenario>()
             .WithMany()
             .HasForeignKey(x => x.LibraryScenarioId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<Scenario>()
             .WithMany()
             .HasForeignKey(x => x.ScenarioId)
             .OnDelete(DeleteBehavior.Restrict);

            // Ein Scenario gehört genau zu einem Mapping (pro Customer)
            e.HasIndex(x => x.ScenarioId).IsUnique();

            e.HasIndex(x => x.LibraryScenarioId);
        }
    }
}
