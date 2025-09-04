using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations
{
    public class LibraryControlScenarioConfiguration : IEntityTypeConfiguration<LibraryControlScenario>
    {
        public void Configure(EntityTypeBuilder<LibraryControlScenario> e)
        {
            e.ToTable("LibraryControlScenarios");
            e.HasKey(x => new { x.LibraryControlId, x.LibraryScenarioId });

            e.HasOne(x => x.LibraryControl)
             .WithMany(lc => lc.ScenarioLinks)
             .HasForeignKey(x => x.LibraryControlId)
             .OnDelete(DeleteBehavior.Cascade);

            // ✅ richtige Rücknavigation setzen, sonst entsteht Shadow-FK LibraryScenarioId1
            e.HasOne(x => x.LibraryScenario)
             .WithMany(ls => ls.ControlLinks)
             .HasForeignKey(x => x.LibraryScenarioId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.LibraryScenarioId);
        }
    }
}
