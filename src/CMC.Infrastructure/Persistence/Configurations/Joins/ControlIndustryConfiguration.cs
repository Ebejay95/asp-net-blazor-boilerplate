using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations
{
    public class ControlIndustryConfiguration : IEntityTypeConfiguration<ControlIndustry>
    {
        public void Configure(EntityTypeBuilder<ControlIndustry> e)
        {
            e.ToTable("ControlIndustries");
            e.HasKey(x => new { x.ControlId, x.IndustryId });

            e.HasOne(x => x.Control)
             .WithMany(c => c.IndustryLinks)
             .HasForeignKey(x => x.ControlId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Industry)
             .WithMany(i => i.ControlIndustries)
             .HasForeignKey(x => x.IndustryId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.IndustryId);
        }
    }
}
