using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations
{
    public sealed class ControlScenarioConfiguration : IEntityTypeConfiguration<ControlScenario>
    {
        public void Configure(EntityTypeBuilder<ControlScenario> e)
        {
            e.ToTable("ControlScenarios");
            e.HasKey(x => new { x.ControlId, x.ScenarioId });

            e.HasOne(x => x.Control)
             .WithMany(c => c.ScenarioLinks)
             .HasForeignKey(x => x.ControlId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Scenario)
             .WithMany(s => s.ControlLinks)
             .HasForeignKey(x => x.ScenarioId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.ScenarioId);
        }
    }
}
