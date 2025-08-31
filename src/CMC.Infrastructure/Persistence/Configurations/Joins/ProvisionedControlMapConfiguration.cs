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

            e.HasOne<Customer>()
             .WithMany()
             .HasForeignKey(x => x.CustomerId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<LibraryControl>()
             .WithMany()
             .HasForeignKey(x => x.LibraryControlId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<Scenario>()
             .WithMany()
             .HasForeignKey(x => x.ScenarioId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<Control>()
             .WithMany()
             .HasForeignKey(x => x.ControlId)
             .OnDelete(DeleteBehavior.Restrict);

            // Ein Control gehört genau zu einem Mapping
            e.HasIndex(x => x.ControlId).IsUnique();

            e.HasIndex(x => x.LibraryControlId);
            e.HasIndex(x => x.ScenarioId);
        }
    }
}
